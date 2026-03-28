// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/03_Core/Interaction/InteractionSystem.cs
// 交互系统。检测范围内的 IInteractable 并管理交互流程。
// ══════════════════════════════════════════════════════════════════════
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 中央交互管理系统。
///
/// 核心职责：
///   · 维护当前可交互目标列表
///   · 自动选择最近的可交互物体作为当前目标
///   · 执行交互并通过 EventBus 广播结果
///   · 提供 UI 层查询当前交互目标的接口
///
/// 使用方式：
///   · 04_Gameplay 层的检测组件（如 InteractionDetector）调用 Register/Unregister
///     将进入/离开范围的 IInteractable 注册到本系统
///   · 玩家按下交互键时调用 TryInteract()
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("交互设置")]
    [SerializeField] private float _maxInteractionDistance = 2f;

    // ══════════════════════════════════════════════════════
    // 字段
    // ══════════════════════════════════════════════════════

    /// <summary>当前范围内的可交互对象列表</summary>
    private readonly List<IInteractable> _nearbyInteractables = new List<IInteractable>();

    /// <summary>当前选中的交互目标</summary>
    private IInteractable _currentTarget;

    /// <summary>交互者（通常是玩家）</summary>
    private Transform _interactor;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    /// <summary>当前交互目标（可能为 null）</summary>
    public IInteractable CurrentTarget => _currentTarget;

    /// <summary>是否有可用的交互目标</summary>
    public bool HasTarget => _currentTarget != null;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        ServiceLocator.Register<InteractionSystem>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<InteractionSystem>();
    }

    private void Update()
    {
        UpdateCurrentTarget();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— 设置交互者
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// 设置交互者（通常在玩家角色初始化时调用）。
    /// </summary>
    public void SetInteractor(Transform interactor)
    {
        _interactor = interactor;
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— 注册/注销可交互对象
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// 注册一个可交互对象（进入检测范围时调用）。
    /// </summary>
    public void RegisterInteractable(IInteractable interactable)
    {
        if (interactable == null) return;
        if (_nearbyInteractables.Contains(interactable)) return;
        _nearbyInteractables.Add(interactable);
    }

    /// <summary>
    /// 注销一个可交互对象（离开检测范围或被销毁时调用）。
    /// </summary>
    public void UnregisterInteractable(IInteractable interactable)
    {
        if (interactable == null) return;
        _nearbyInteractables.Remove(interactable);

        // 如果移除的是当前目标，清空并通知
        if (_currentTarget == interactable)
        {
            _currentTarget = null;
            EventBus.Publish(new InteractableLostEvent());
        }
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— 执行交互
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// 尝试与当前目标交互。
    /// </summary>
    /// <returns>是否成功执行交互</returns>
    public bool TryInteract()
    {
        if (_currentTarget == null) return false;
        if (_interactor == null) return false;

        if (!_currentTarget.CanInteract(_interactor.gameObject))
            return false;

        _currentTarget.Interact(_interactor.gameObject);

        EventBus.Publish(new InteractionPerformedEvent
        {
            Type = _currentTarget.InteractionType,
            InteractableId = _currentTarget.Transform != null
                ? _currentTarget.Transform.name
                : string.Empty
        });

        return true;
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    /// <summary>每帧更新当前最佳交互目标</summary>
    private void UpdateCurrentTarget()
    {
        if (_interactor == null)
        {
            if (_currentTarget != null)
            {
                _currentTarget = null;
                EventBus.Publish(new InteractableLostEvent());
            }
            return;
        }

        // [PERF] 清理无效引用（物体被销毁等情况）
        for (int i = _nearbyInteractables.Count - 1; i >= 0; i--)
        {
            var interactable = _nearbyInteractables[i];
            if (interactable == null || interactable.Transform == null)
            {
                _nearbyInteractables.RemoveAt(i);
            }
        }

        // [PERF] 找到最近的可交互对象
        IInteractable closest = null;
        float closestDist = _maxInteractionDistance;
        Vector3 interactorPos = _interactor.position;

        for (int i = 0; i < _nearbyInteractables.Count; i++)
        {
            var interactable = _nearbyInteractables[i];
            if (!interactable.CanInteract(_interactor.gameObject)) continue;

            float dist = Vector3.Distance(interactorPos, interactable.Transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = interactable;
            }
        }

        // 目标变更通知
        if (closest != _currentTarget)
        {
            var previousTarget = _currentTarget;
            _currentTarget = closest;

            if (_currentTarget != null)
            {
                EventBus.Publish(new InteractableDetectedEvent
                {
                    Type = _currentTarget.InteractionType,
                    Prompt = _currentTarget.InteractionPrompt
                });
            }
            else if (previousTarget != null)
            {
                EventBus.Publish(new InteractableLostEvent());
            }
        }
    }
}
