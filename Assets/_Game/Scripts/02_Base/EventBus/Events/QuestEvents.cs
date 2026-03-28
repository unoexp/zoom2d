// ─────────────────────────────────────────────────────────────────────
// 📁 Assets/_Game/02_Base/EventBus/Events/QuestEvents.cs
// 任务系统事件定义
// ─────────────────────────────────────────────────────────────────────

/// <summary>任务激活/接取事件</summary>
public struct QuestActivatedEvent : IEvent
{
    public string QuestId;
    public string DisplayName;
    public bool IsMainQuest;
}

/// <summary>任务目标进度更新事件</summary>
public struct QuestObjectiveProgressEvent : IEvent
{
    public string QuestId;
    public int ObjectiveIndex;
    public int CurrentAmount;
    public int RequiredAmount;
    public bool IsCompleted;
}

/// <summary>任务完成事件</summary>
public struct QuestCompletedEvent : IEvent
{
    public string QuestId;
    public string DisplayName;
    public bool IsMainQuest;
}

/// <summary>任务失败事件</summary>
public struct QuestFailedEvent : IEvent
{
    public string QuestId;
    public string Reason;
}

/// <summary>任务目标进度检查请求事件（各系统发布，QuestSystem订阅）</summary>
public struct QuestProgressCheckEvent : IEvent
{
    /// <summary>目标类型</summary>
    public QuestObjectiveType ObjectiveType;

    /// <summary>相关ID（物品ID/建筑ID/敌人类型/NPC ID等）</summary>
    public string TargetId;

    /// <summary>变化量（+1 = 新增一个）</summary>
    public int Amount;
}
