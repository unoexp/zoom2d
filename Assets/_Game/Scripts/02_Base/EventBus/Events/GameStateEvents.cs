// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Base/EventBus/Events/GameStateEvents.cs
// 游戏状态变更事件定义
// ══════════════════════════════════════════════════════════════════════

/// <summary>
/// 游戏全局状态切换事件。
/// GameStateManager 在每次状态变更时发布。
/// </summary>
public struct GameStateChangedEvent : IEvent
{
    /// <summary>切换前的状态</summary>
    public GameState PreviousState;

    /// <summary>切换后的新状态</summary>
    public GameState NewState;
}
