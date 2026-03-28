// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/NPC/NPCController.cs
// NPC 控制器。管理 NPC 行为、对话触发、信任度。
// ══════════════════════════════════════════════════════════════════════
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC 控制器。
///
/// 核心职责：
///   · 持有 NPCDefinitionSO 数据引用
///   · 实现 IInteractable，响应玩家交互
///   · 管理信任度系统
///   · 驱动对话流程（DialogSystem 处理具体对话逻辑）
///   · 根据行为倾向决定交互方式
///
/// 设计说明：
///   · 挂载在 NPC 的 GameObject 上
///   · 出现条件由 SpawnManager 根据 NPCDefinitionSO 判断
///   · 信任度变化通过 EventBus 广播
/// </summary>
public class NPCController : MonoBehaviour, IInteractable
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("NPC 数据")]
    [SerializeField] private NPCDefinitionSO _definition;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    private int _currentTrust;
    private NPCDisposition _currentDisposition;
    private bool _isInDialog;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    public NPCDefinitionSO Definition => _definition;
    public int CurrentTrust => _currentTrust;
    public NPCDisposition CurrentDisposition => _currentDisposition;
    public bool IsInDialog => _isInDialog;

    // ══════════════════════════════════════════════════════
    // IInteractable 实现
    // ══════════════════════════════════════════════════════

    public InteractionType InteractionType => InteractionType.Talk;
    public string InteractionPrompt =>
        _definition != null ? $"与 {_definition.DisplayName} 交谈" : "交谈";
    public bool CanInteract(GameObject interactor)
    {
        return !_isInDialog && _currentDisposition != NPCDisposition.Hostile;
    }
    public Transform Transform => transform;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        StartDialog();
    }

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        if (_definition != null)
        {
            _currentDisposition = _definition.DefaultDisposition;
            _currentTrust = 0;
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<DialogEndedEvent>(OnDialogEnded);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DialogEndedEvent>(OnDialogEnded);
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>修改信任度</summary>
    public void ModifyTrust(int delta)
    {
        int oldTrust = _currentTrust;
        _currentTrust = Mathf.Clamp(_currentTrust + delta, 0, 100);

        // 检查是否因信任度变化改变行为倾向
        if (_definition != null && _currentDisposition == NPCDisposition.Neutral
            && _currentTrust >= _definition.TrustThreshold)
        {
            _currentDisposition = NPCDisposition.Friendly;
        }

        EventBus.Publish(new NPCTrustChangedEvent
        {
            NPCId = _definition != null ? _definition.NPCId : "",
            OldTrust = oldTrust,
            NewTrust = _currentTrust
        });
    }

    /// <summary>设置行为倾向（难度系统可调用）</summary>
    public void SetDisposition(NPCDisposition disposition)
    {
        _currentDisposition = disposition;
    }

    // ══════════════════════════════════════════════════════
    // 对话控制
    // ══════════════════════════════════════════════════════

    /// <summary>开始对话</summary>
    private void StartDialog()
    {
        if (_definition == null || _definition.DefaultDialog == null) return;

        _isInDialog = true;

        EventBus.Publish(new DialogStartedEvent
        {
            DialogId = _definition.DefaultDialog.DialogId,
            NPCId = _definition.NPCId
        });

        // 通知 PlayerFacade 禁用输入
        if (ServiceLocator.TryGet<PlayerFacade>(out var player))
        {
            player.BeginInteraction();
        }
    }

    /// <summary>对话结束回调</summary>
    private void OnDialogEnded(DialogEndedEvent evt)
    {
        if (_definition == null) return;
        if (evt.NPCId != _definition.NPCId) return;

        _isInDialog = false;

        if (ServiceLocator.TryGet<PlayerFacade>(out var player))
        {
            player.EndInteraction();
        }
    }
}
