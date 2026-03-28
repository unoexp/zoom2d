// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Player/PlayerFacade.cs
// 玩家门面类。整合所有玩家子系统的统一访问入口。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 玩家门面类（Facade）。
///
/// 核心职责：
///   · 整合 PlayerController、SurvivalStatusSystem、EquipmentSystem 等子系统
///   · 为外部系统（敌人AI、NPC、UI）提供统一的玩家状态查询接口
///   · 管理玩家在庇护所内/外的状态切换
///   · 注册到 ServiceLocator 供全局访问
///
/// 设计说明：
///   · 不重复子系统逻辑，仅做聚合和委托
///   · 挂载在玩家 GameObject 上，引用通过 GetComponent 获取
///   · 04_Gameplay 层可引用 03_Core 和 02_Base
/// </summary>
public class PlayerFacade : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 子系统引用
    // ══════════════════════════════════════════════════════

    private PlayerController _controller;
    private SurvivalStatusSystem _survivalSystem;
    private EquipmentSystem _equipmentSystem;
    private TemperatureSystem _temperatureSystem;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    private bool _isInShelter;
    private bool _isInteracting;

    // ══════════════════════════════════════════════════════
    // 属性 —— 状态查询
    // ══════════════════════════════════════════════════════

    /// <summary>玩家控制器</summary>
    public PlayerController Controller => _controller;

    /// <summary>是否在庇护所内</summary>
    public bool IsInShelter => _isInShelter;

    /// <summary>是否正在交互</summary>
    public bool IsInteracting => _isInteracting;

    /// <summary>是否存活</summary>
    public bool IsAlive => _controller != null && !_controller.IsDead;

    /// <summary>当前行为状态</summary>
    public PlayerState CurrentState => _controller != null ? _controller.CurrentState : PlayerState.Idle;

    /// <summary>世界坐标</summary>
    public Vector2 Position => transform.position;

    /// <summary>面朝方向（true=右）</summary>
    public bool FacingRight => _controller != null && _controller.FacingRight;

    // ══════════════════════════════════════════════════════
    // 属性 —— 生存状态
    // ══════════════════════════════════════════════════════

    /// <summary>血量百分比 0~1</summary>
    public float HealthPercent =>
        _survivalSystem != null ? _survivalSystem.GetNormalized(SurvivalAttributeType.Health) : 1f;

    /// <summary>饥饿值百分比 0~1</summary>
    public float HungerPercent =>
        _survivalSystem != null ? _survivalSystem.GetNormalized(SurvivalAttributeType.Hunger) : 1f;

    /// <summary>口渴值百分比 0~1</summary>
    public float ThirstPercent =>
        _survivalSystem != null ? _survivalSystem.GetNormalized(SurvivalAttributeType.Thirst) : 1f;

    /// <summary>体力百分比 0~1</summary>
    public float StaminaPercent =>
        _survivalSystem != null ? _survivalSystem.GetNormalized(SurvivalAttributeType.Stamina) : 1f;

    /// <summary>体温百分比 0~1</summary>
    public float TemperaturePercent =>
        _survivalSystem != null ? _survivalSystem.GetNormalized(SurvivalAttributeType.Temperature) : 1f;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        ServiceLocator.Register<PlayerFacade>(this);
    }

    private void Start()
    {
        _survivalSystem = ServiceLocator.Get<SurvivalStatusSystem>();
        _equipmentSystem = ServiceLocator.Get<EquipmentSystem>();

        if (ServiceLocator.TryGet<TemperatureSystem>(out var tempSys))
            _temperatureSystem = tempSys;
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlayerFacade>();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— 状态切换
    // ══════════════════════════════════════════════════════

    /// <summary>进入庇护所</summary>
    public void EnterShelter()
    {
        if (_isInShelter) return;
        _isInShelter = true;

        // 通知温度系统
        if (_temperatureSystem != null)
            _temperatureSystem.SetInShelter(true);

        EventBus.Publish(new PlayerEnteredShelterEvent());
        Debug.Log("[PlayerFacade] 进入庇护所");
    }

    /// <summary>离开庇护所</summary>
    public void LeaveShelter()
    {
        if (!_isInShelter) return;
        _isInShelter = false;

        if (_temperatureSystem != null)
            _temperatureSystem.SetInShelter(false);

        EventBus.Publish(new PlayerLeftShelterEvent());
        Debug.Log("[PlayerFacade] 离开庇护所");
    }

    /// <summary>开始交互（禁用移动输入）</summary>
    public void BeginInteraction()
    {
        _isInteracting = true;
        if (_controller != null)
            _controller.DisableInput();
    }

    /// <summary>结束交互</summary>
    public void EndInteraction()
    {
        _isInteracting = false;
        if (_controller != null)
            _controller.EnableInput();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— 生存属性快捷方法
    // ══════════════════════════════════════════════════════

    /// <summary>消耗体力</summary>
    public void ConsumeStamina(float amount)
    {
        if (_survivalSystem != null)
            _survivalSystem.ModifyAttribute(SurvivalAttributeType.Stamina, -amount);
    }

    /// <summary>恢复体力</summary>
    public void RestoreStamina(float amount)
    {
        if (_survivalSystem != null)
            _survivalSystem.ModifyAttribute(SurvivalAttributeType.Stamina, amount);
    }

    /// <summary>检查是否有足够体力</summary>
    public bool HasStamina(float required)
    {
        return _survivalSystem != null &&
               _survivalSystem.GetValue(SurvivalAttributeType.Stamina) >= required;
    }
}

// ══════════════════════════════════════════════════════════════════════
// 庇护所相关事件（轻量，定义在此处避免单独文件）
// ══════════════════════════════════════════════════════════════════════

/// <summary>玩家进入庇护所事件</summary>
public struct PlayerEnteredShelterEvent : IEvent { }

/// <summary>玩家离开庇护所事件</summary>
public struct PlayerLeftShelterEvent : IEvent { }
