// ─────────────────────────────────────────────────────────────────────
// 📁 Assets/_Game/02_Base/EventBus/Events/TradingEvents.cs
// 交易系统事件定义
// ─────────────────────────────────────────────────────────────────────

/// <summary>打开交易面板请求</summary>
public struct TradeOpenRequestEvent : IEvent
{
    public string NPCId;
    public string OfferId;
}

/// <summary>交易执行事件（购买/出售）</summary>
public struct TradeExecutedEvent : IEvent
{
    public string ItemId;
    public int Amount;
    public int TotalPrice;
    public bool PlayerBuying;   // true=玩家购买, false=玩家出售
    public string NPCId;
}

/// <summary>交易失败事件</summary>
public struct TradeFailedEvent : IEvent
{
    public string ItemId;
    public string Reason;
}

/// <summary>交易面板关闭事件</summary>
public struct TradeClosedEvent : IEvent
{
    public string NPCId;
}
