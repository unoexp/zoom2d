// ─────────────────────────────────────────────────────────────────────
// 📁 Assets/_Game/02_Base/EventBus/Events/LoadingEvents.cs
// 加载系统事件定义
// ─────────────────────────────────────────────────────────────────────

/// <summary>加载开始事件</summary>
public struct LoadingStartedEvent : IEvent
{
    /// <summary>加载提示文本</summary>
    public string HintText;
}

/// <summary>加载进度更新事件</summary>
public struct LoadingProgressEvent : IEvent
{
    /// <summary>加载进度（0-1）</summary>
    public float Progress;

    /// <summary>当前步骤描述</summary>
    public string StepDescription;
}

/// <summary>加载完成事件</summary>
public struct LoadingCompletedEvent : IEvent { }
