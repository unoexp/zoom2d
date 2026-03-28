// ─────────────────────────────────────────────────────────────────────
// 📁 Assets/_Game/02_Base/EventBus/Events/ItemEffectEvents.cs
// 物品效果相关事件（消耗品使用结果、状态效果请求等）
// ─────────────────────────────────────────────────────────────────────

/// <summary>消耗品使用完成事件（通知 05_Show 播放反馈动画）</summary>
public struct ConsumableUsedEvent : IEvent
{
    public string ItemId;
    public string DisplayName;
    public int EffectCount;
}

/// <summary>请求施加状态效果事件（由物品/技能等系统发布）</summary>
public struct StatusEffectRequestEvent : IEvent
{
    public string EffectId;
    public float Duration;
}
