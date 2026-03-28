// ─────────────────────────────────────────────────────────────────────
// 📁 Assets/_Game/05_Show/UI/Events/UIManagerEvents.cs
// UI管理器相关事件（面板打开/关闭通知）
// ─────────────────────────────────────────────────────────────────────

/// <summary>UI面板被打开</summary>
public struct UIPanelOpenedEvent : IEvent
{
    public string PanelId;
}

/// <summary>UI面板被关闭</summary>
public struct UIPanelClosedEvent : IEvent
{
    public string PanelId;
}

/// <summary>所有UI面板被关闭</summary>
public struct UIAllPanelsClosedEvent : IEvent { }
