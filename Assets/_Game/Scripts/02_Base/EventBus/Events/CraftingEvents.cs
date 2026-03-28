// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Base/EventBus/Events/CraftingEvents.cs
// 制作系统事件定义
// ══════════════════════════════════════════════════════════════════════

/// <summary>制作请求事件（UI发起）</summary>
public struct CraftingRequestEvent : IEvent
{
    public string RecipeId;
    public int Amount;
}

/// <summary>制作结果事件</summary>
public struct CraftingResultEvent : IEvent
{
    public string RecipeId;
    public CraftingResult Result;
    public string OutputItemId;
    public int OutputAmount;
}

/// <summary>配方解锁事件</summary>
public struct RecipeUnlockedEvent : IEvent
{
    public string RecipeId;
}
