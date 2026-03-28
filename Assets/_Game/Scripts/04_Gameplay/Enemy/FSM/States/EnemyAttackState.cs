// 📁 Assets/_Game/04_Gameplay/Enemy/FSM/States/EnemyAttackState.cs
// 攻击状态：在攻击范围内对目标造成伤害
using UnityEngine;

public class EnemyAttackState : EnemyStateBase
{
    private float _attackTimer;

    public EnemyAttackState(EnemyBase enemy, EnemyStateMachine fsm) : base(enemy, fsm) { }

    public override void OnEnter()
    {
        Enemy.StopMoving();
        Enemy.SetAnimationState("Attack");
        _attackTimer = 0f;

        // 立即执行一次攻击
        PerformAttack();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (Enemy.IsDead) return;
        if (Enemy.Definition == null) return;

        float dist = Enemy.DistanceToTarget;

        // 目标离开攻击范围，重新追击
        if (dist > Enemy.Definition.AttackRange * 1.2f)
        {
            FSM.ChangeState(EnemyState.Chase);
            return;
        }

        // 低血量逃跑
        if (Enemy.HealthPercent <= Enemy.Definition.FleeHealthThreshold)
        {
            FSM.ChangeState(EnemyState.Flee);
            return;
        }

        // 攻击冷却
        _attackTimer += deltaTime;
        if (_attackTimer >= Enemy.Definition.AttackCooldown)
        {
            _attackTimer = 0f;
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (Enemy.Target == null) return;

        var target = Enemy.Target.GetComponent<IDamageable>();
        if (target == null || target.IsDead) return;

        if (ServiceLocator.TryGet<CombatSystem>(out var combat))
        {
            combat.Attack(
                Enemy.gameObject,
                target,
                Enemy.Definition.AttackDamage,
                Enemy.Definition.DamageType);
        }
    }
}
