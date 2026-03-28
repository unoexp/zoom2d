// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Enemy/FSM/EnemyStateMachine.cs
// 敌人状态机。封装 StateMachine<EnemyState>，注册所有敌人行为状态。
// ══════════════════════════════════════════════════════════════════════

/// <summary>
/// 敌人状态机管理器。
/// 由 EnemyBase.Update/FixedUpdate 驱动。
/// </summary>
public class EnemyStateMachine
{
    private readonly StateMachine<EnemyState> _fsm = new StateMachine<EnemyState>();

    public EnemyState CurrentState => _fsm.CurrentStateKey;

    public EnemyStateMachine(EnemyBase enemy)
    {
        _fsm.AddState(EnemyState.Idle,        new EnemyIdleState(enemy, this));
        _fsm.AddState(EnemyState.Patrol,      new EnemyPatrolState(enemy, this));
        _fsm.AddState(EnemyState.Chase,       new EnemyChaseState(enemy, this));
        _fsm.AddState(EnemyState.Attack,      new EnemyAttackState(enemy, this));
        _fsm.AddState(EnemyState.Flee,        new EnemyFleeState(enemy, this));
        _fsm.AddState(EnemyState.Dead,        new EnemyDeadState(enemy, this));
    }

    public void ChangeState(EnemyState state) => _fsm.ChangeState(state);
    public void Update(float deltaTime) => _fsm.Update(deltaTime);
    public void FixedUpdate(float fixedDeltaTime) => _fsm.FixedUpdate(fixedDeltaTime);

    public void Initialize(EnemyState initialState = EnemyState.Idle)
    {
        _fsm.ChangeState(initialState);
    }
}
