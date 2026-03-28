// 📁 Assets/_Game/04_Gameplay/Enemy/FSM/States/EnemyIdleState.cs
// 待机状态：原地不动，检测玩家进入视野
using UnityEngine;

public class EnemyIdleState : EnemyStateBase
{
    private float _idleTimer;
    private float _idleDuration;

    public EnemyIdleState(EnemyBase enemy, EnemyStateMachine fsm) : base(enemy, fsm) { }

    public override void OnEnter()
    {
        Enemy.StopMoving();
        Enemy.SetAnimationState("Idle");
        _idleTimer = 0f;
        _idleDuration = Random.Range(2f, 5f);
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

        // 待机一段时间后进入巡逻
        _idleTimer += deltaTime;
        if (_idleTimer >= _idleDuration)
        {
            FSM.ChangeState(EnemyState.Patrol);
        }
    }
}
