// ─────────────────────────────────────────────────────────────────────
// 📁 Assets/_Game/02_Base/EventBus/Events/BuildingEvents.cs
// 建造系统事件定义
// ─────────────────────────────────────────────────────────────────────

/// <summary>建造请求事件（UI发起）</summary>
public struct BuildRequestEvent : IEvent
{
    public string BuildingId;
}

/// <summary>建造完成事件</summary>
public struct BuildCompletedEvent : IEvent
{
    public string BuildingId;
    public string DisplayName;
    public UnityEngine.Vector2 Position;
}

/// <summary>建筑拆除事件</summary>
public struct BuildingDemolishedEvent : IEvent
{
    public string BuildingId;
    public UnityEngine.Vector2 Position;
}

/// <summary>庇护所升级阶段变化事件</summary>
public struct ShelterStageChangedEvent : IEvent
{
    public int OldStage;
    public int NewStage;
}

/// <summary>建筑解锁事件</summary>
public struct BuildingUnlockedEvent : IEvent
{
    public string BuildingId;
}
