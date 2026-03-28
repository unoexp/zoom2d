// 📁 Assets/_Game/04_Gameplay/Enemy/FSM/States/EnemyPatrolState.cs
// 巡逻状态：在一定范围内来回移动
using UnityEngine;

public class EnemyPatrolState : EnemyStateBase
{
    private float _patrolTimer;
    private float _patrolDuration;
    private int _direction;

    public EnemyPatrolState(EnemyBase enemy, EnemyStateMachine fsm) : base(enemy, fsm) { }

    public override void OnEnter()
    {
        Enemy.SetAnimationState("Walk");
        _patrolTimer = 0f;
        _patrolDuration = Random.Range(2f, 6f);
        _direction = Random.value > 0.5f ? 1 : -1;
    }

    public override void OnUpdate(float deltaTime)
    {
        if (Enemy.IsDead) return;

        // 检测玩家
        if (Enemy.Definition != null && Enemy.DistanceToTarget <= Enemy.Definition.DetectionRange)
        {
            FSM.ChangeState(EnemyState.Chase);
            return;
        }

        // 巡逻移动
        float speed = Enemy.Definition != null
            ? Enemy.Definition.MoveSpeed * Enemy.Definition.PatrolSpeedMultiplier
            : 1.5f;

        Enemy.Rb.velocity = new Vector2(_direction * speed, Enemy.Rb.velocity.y);

        // 巡逻时间到后回到待机
        _patrolTimer += deltaTime;
        if (_patrolTimer >= _patrolDuration)
        {
            FSM.ChangeState(EnemyState.Idle);
        }
    }

    public override void OnExit()
    {
        Enemy.StopMoving();
    }
}
