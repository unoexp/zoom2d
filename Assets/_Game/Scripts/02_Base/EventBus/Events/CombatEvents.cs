// 📁 02_Infrastructure/EventBus/Events/CombatEvents.cs

/// <summary>伤害事件</summary>
public struct DamageDealtEvent : IEvent
{
    public int SourceInstanceId;    // 攻击者GameObject InstanceID
    public int TargetInstanceId;    // 受击者GameObject InstanceID
    public float DamageAmount;
    public DamageType DamageType;
    public bool IsCritical;
}