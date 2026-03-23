// 📁 02_Infrastructure/EventBus/Events/InventoryEvents.cs
// ⚠️ 所有事件定义为结构体，零GC分配

/// <summary>物品被添加到背包</summary>
public struct ItemAddedToInventoryEvent : IEvent
{
    public string ItemId;       // 物品ID（对应ItemDefinitionSO）
    public int Amount;
    public int SlotIndex;       // 放入的槽位索引
}

/// <summary>物品被从背包移除</summary>
public struct ItemRemovedFromInventoryEvent : IEvent
{
    public string ItemId;
    public int Amount;
}

/// <summary>背包已满</summary>
public struct InventoryFullEvent : IEvent { }