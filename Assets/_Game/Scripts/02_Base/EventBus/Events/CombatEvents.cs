// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Base/EventBus/Events/CombatEvents.cs
// 战斗系统事件定义
// ══════════════════════════════════════════════════════════════════════

/// <summary>伤害事件（伤害计算完成后发布）</summary>
public struct DamageDealtEvent : IEvent
{
    public int SourceInstanceId;    // 攻击者 GameObject InstanceID
    public int TargetInstanceId;    // 受击者 GameObject InstanceID
    public float DamageAmount;
    public DamageType DamageType;
    public bool IsCritical;
}

/// <summary>攻击发起事件（攻击动作开始时发布，用于音效/动画）</summary>
public struct AttackStartedEvent : IEvent
{
    public int AttackerInstanceId;
    public int TargetInstanceId;
    public DamageType DamageType;
}

/// <summary>实体死亡事件（生命值归零时发布）</summary>
public struct EntityDiedEvent : IEvent
{
    public int EntityInstanceId;
    public int KillerInstanceId;    // 击杀者 InstanceID（0 = 环境伤害）
    public DeathCause Cause;
}

/// <summary>实体受伤事件（UI血条、受击闪烁等表现层使用）</summary>
public struct EntityHitEvent : IEvent
{
    public int EntityInstanceId;
    public float DamageAmount;
    public float RemainingHealth;
    public float MaxHealth;
    public bool IsCritical;
}

/// <summary>实体生命值变化事件（治疗/伤害均触发）</summary>
public struct HealthChangedEvent : IEvent
{
    public int EntityInstanceId;
    public float OldHealth;
    public float NewHealth;
    public float MaxHealth;
}