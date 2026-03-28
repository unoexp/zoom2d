// ─────────────────────────────────────────────────────────────────────
// 📁 Assets/_Game/02_Base/EventBus/Events/CurrencyEvents.cs
// 货币系统事件定义
// ─────────────────────────────────────────────────────────────────────

/// <summary>货币变化事件</summary>
public struct CurrencyChangedEvent : IEvent
{
    public int OldAmount;
    public int NewAmount;
    public int Delta;           // 正数为增加，负数为减少
    public string Reason;       // 变化原因（如 "交易"、"任务奖励"）
}

/// <summary>货币不足事件（尝试消费但余额不够时触发）</summary>
public struct CurrencyInsufficientEvent : IEvent
{
    public int Required;
    public int Current;
}
