// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Player/FSM/PlayerStateBase.cs
// 玩家状态基类。提供对 PlayerController 和 PlayerStateMachine 的便捷访问。
// ══════════════════════════════════════════════════════════════════════

/// <summary>
/// 所有玩家状态的基类。
/// 持有 Player 和 FSM 引用，子类只需关注具体行为逻辑。
/// </summary>
public abstract class PlayerStateBase : IState
{
    protected readonly PlayerController Player;
    protected readonly PlayerStateMachine FSM;

    protected PlayerStateBase(PlayerController player, PlayerStateMachine fsm)
    {
        Player = player;
        FSM = fsm;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate(float deltaTime) { }
    public virtual void OnFixedUpdate(float fixedDeltaTime) { }
    public virtual void OnExit() { }
}
