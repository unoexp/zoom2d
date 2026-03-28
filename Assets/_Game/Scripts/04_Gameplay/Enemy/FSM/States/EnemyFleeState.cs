// 📁 Assets/_Game/04_Gameplay/Enemy/FSM/States/EnemyFleeState.cs
// 逃跑状态：血量过低时远离玩家
using UnityEngine;

public class EnemyFleeState : EnemyStateBase
{
    private float _fleeTimer;
    private const float FLEE_DURATION = 5f;

    public EnemyFleeState(EnemyBase enemy, EnemyStateMachine fsm) : base(enemy, fsm) { }

    public override void OnEnter()
    {
        Enemy.SetAnimationState("Run");
        _fleeTimer = 0f;
    }

    public override void OnUpdate(float deltaTime)
    {
        if (Enemy.IsDead) return;

        // 远离目标
        if (Enemy.Target != null && Enemy.Definition != null)
        {
            float dir = Mathf.Sign(Enemy.Transform.position.x - Enemy.Target.position.x);
            Enemy.Rb.velocity = new Vector2(dir * Enemy.Definition.MoveSpeed, Enemy.Rb.velocity.y);
        }

        _fleeTimer += deltaTime;

        // 逃跑一段时间后，如果距离足够远或超时，回到待机
        float chaseRange = Enemy.Definition != null ? Enemy.Definition.ChaseRange : 15f;
        if (_fleeTimer >= FLEE_DURATION || Enemy.DistanceToTarget > chaseRange)
        {
            FSM.ChangeState(EnemyState.Idle);
        }
    }

    public override void OnExit()
    {
        Enemy.StopMoving();
    }
}
