// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Base/EventBus/Events/InteractionEvents.cs
// 交互系统事件定义
// ══════════════════════════════════════════════════════════════════════

/// <summary>交互目标进入范围</summary>
public struct InteractableDetectedEvent : IEvent
{
    public InteractionType Type;
    public string Prompt;
}

/// <summary>交互目标离开范围</summary>
public struct InteractableLostEvent : IEvent { }

/// <summary>交互执行完成</summary>
public struct InteractionPerformedEvent : IEvent
{
    public InteractionType Type;
    public string InteractableId;
}
