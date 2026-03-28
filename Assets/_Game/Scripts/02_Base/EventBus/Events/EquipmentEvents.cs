// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Base/EventBus/Events/EquipmentEvents.cs
// 装备系统事件定义
// ══════════════════════════════════════════════════════════════════════

/// <summary>装备穿戴事件</summary>
public struct ItemEquippedEvent : IEvent
{
    public EquipmentSlot Slot;
    public string ItemId;
}

/// <summary>装备卸下事件</summary>
public struct ItemUnequippedEvent : IEvent
{
    public EquipmentSlot Slot;
    public string PreviousItemId;
}

/// <summary>玩家总防御力变化事件</summary>
public struct PlayerDefenseChangedEvent : IEvent
{
    public float TotalDefense;
}
