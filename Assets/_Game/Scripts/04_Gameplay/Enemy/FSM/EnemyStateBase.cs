// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Enemy/FSM/EnemyStateBase.cs
// 敌人状态基类。提供对 EnemyBase 和 EnemyStateMachine 的便捷访问。
// ══════════════════════════════════════════════════════════════════════

/// <summary>
/// 所有敌人状态的基类。
/// </summary>
public abstract class EnemyStateBase : IState
{
    protected readonly EnemyBase Enemy;
    protected readonly EnemyStateMachine FSM;

    protected EnemyStateBase(EnemyBase enemy, EnemyStateMachine fsm)
    {
        Enemy = enemy;
        FSM = fsm;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate(float deltaTime) { }
    public virtual void OnFixedUpdate(float fixedDeltaTime) { }
    public virtual void OnExit() { }
}
