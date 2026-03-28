// ─────────────────────────────────────────────────────────────────────
// 📁 Assets/_Game/02_Base/EventBus/Events/DiggingEvents.cs
// 挖掘系统事件定义
// ─────────────────────────────────────────────────────────────────────

/// <summary>挖掘开始事件</summary>
public struct DiggingStartedEvent : IEvent
{
    public UnityEngine.Vector2Int TilePosition;
    public string LayerId;
    public float Duration;
}

/// <summary>挖掘完成事件</summary>
public struct DiggingCompletedEvent : IEvent
{
    public UnityEngine.Vector2Int TilePosition;
    public string LayerId;
}

/// <summary>挖掘失败事件（体力不足等）</summary>
public struct DiggingFailedEvent : IEvent
{
    public string Reason;
}

/// <summary>地层解锁事件</summary>
public struct LayerUnlockedEvent : IEvent
{
    public string LayerId;
    public string LayerName;
}
