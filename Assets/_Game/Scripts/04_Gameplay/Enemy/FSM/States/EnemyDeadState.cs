// 📁 Assets/_Game/04_Gameplay/Enemy/FSM/States/EnemyDeadState.cs
// 死亡状态：播放死亡动画，延迟回收到对象池
using UnityEngine;

public class EnemyDeadState : EnemyStateBase
{
    private float _despawnTimer;
    private const float DESPAWN_DELAY = 3f;

    public EnemyDeadState(EnemyBase enemy, EnemyStateMachine fsm) : base(enemy, fsm) { }

    public override void OnEnter()
    {
        Enemy.SetAnimationState("Dead");
        Enemy.StopMoving();
        _despawnTimer = 0f;
    }

    public override void OnUpdate(float deltaTime)
    {
        _despawnTimer += deltaTime;

        // 延迟后回收到对象池
        if (_despawnTimer >= DESPAWN_DELAY)
        {
            if (ServiceLocator.TryGet<ObjectPoolManager>(out var pool))
                pool.Release(Enemy.gameObject);
            else
                Object.Destroy(Enemy.gameObject);
        }
    }
}
