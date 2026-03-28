// 📁 Assets/_Game/04_Gameplay/Enemy/FSM/States/EnemyChaseState.cs
// 追击状态：向玩家移动，进入攻击范围后切换到攻击

public class EnemyChaseState : EnemyStateBase
{
    public EnemyChaseState(EnemyBase enemy, EnemyStateMachine fsm) : base(enemy, fsm) { }

    public override void OnEnter()
    {
        Enemy.SetAnimationState("Run");
    }

    public override void OnUpdate(float deltaTime)
    {
        if (Enemy.IsDead) return;
        if (Enemy.Definition == null) return;

        // 低血量逃跑
        if (Enemy.HealthPercent <= Enemy.Definition.FleeHealthThreshold)
        {
            FSM.ChangeState(EnemyState.Flee);
            return;
        }

        float dist = Enemy.DistanceToTarget;

        // 超出追击范围，回到待机
        if (dist > Enemy.Definition.ChaseRange)
        {
            FSM.ChangeState(EnemyState.Idle);
            return;
        }

        // 进入攻击范围
        if (dist <= Enemy.Definition.AttackRange)
        {
            FSM.ChangeState(EnemyState.Attack);
            return;
        }

        // 追击移动
        Enemy.MoveTowardsTarget();
    }

    public override void OnExit()
    {
        Enemy.StopMoving();
    }
}
