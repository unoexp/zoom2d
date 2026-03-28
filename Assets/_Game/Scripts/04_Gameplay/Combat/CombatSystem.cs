// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Combat/CombatSystem.cs
// 战斗系统。协调攻击发起、伤害计算、结果广播。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 中央战斗管理系统。
///
/// 核心职责：
///   · 接收攻击请求，通过 DamageCalculator 计算伤害
///   · 将伤害施加到 IDamageable 目标
///   · 通过 EventBus 广播所有战斗事件（攻击、受伤、死亡）
///   · 管理击退效果
///
/// 使用方式：
///   · 攻击者调用 CombatSystem.Attack() 发起攻击
///   · 或通过范围检测调用 CombatSystem.DealDamageInArea()
/// </summary>
public class CombatSystem : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("战斗参数")]
    [Tooltip("默认击退力度")]
    [SerializeField] private float _defaultKnockbackForce = 5f;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        ServiceLocator.Register<CombatSystem>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<CombatSystem>();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— 单体攻击
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// 对单个目标发起攻击。
    /// </summary>
    /// <param name="attacker">攻击者</param>
    /// <param name="target">目标（需实现 IDamageable）</param>
    /// <param name="baseDamage">基础伤害</param>
    /// <param name="damageType">伤害类型</param>
    /// <param name="critChance">暴击率</param>
    public void Attack(GameObject attacker, IDamageable target,
                       float baseDamage, DamageType damageType,
                       float critChance = 0.1f)
    {
        if (target == null || target.IsDead) return;

        // 广播攻击发起
        EventBus.Publish(new AttackStartedEvent
        {
            AttackerInstanceId = attacker != null ? attacker.GetInstanceID() : 0,
            TargetInstanceId = target.Transform.gameObject.GetInstanceID(),
            DamageType = damageType
        });

        // 计算伤害
        var result = DamageCalculator.Calculate(baseDamage, damageType, critChance: critChance);

        // 计算击退方向
        Vector2 knockbackDir = Vector2.zero;
        if (attacker != null && target.Transform != null)
        {
            knockbackDir = (target.Transform.position - attacker.transform.position).normalized;
        }

        // 构建伤害信息
        var damageInfo = new DamageInfo
        {
            Attacker = attacker,
            Damage = result.FinalDamage,
            DamageType = result.DamageType,
            IsCritical = result.IsCritical,
            KnockbackDirection = knockbackDir,
            KnockbackForce = _defaultKnockbackForce
        };

        // 施加伤害
        ApplyDamage(target, damageInfo);
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— 范围攻击
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// 对范围内所有 IDamageable 施加伤害。
    /// </summary>
    /// <param name="attacker">攻击者</param>
    /// <param name="center">范围中心</param>
    /// <param name="radius">范围半径</param>
    /// <param name="baseDamage">基础伤害</param>
    /// <param name="damageType">伤害类型</param>
    /// <param name="targetLayer">目标层级</param>
    /// <summary>范围攻击检测缓冲区</summary>
    private static readonly Collider2D[] _areaHitBuffer = new Collider2D[32];

    public void DealDamageInArea(GameObject attacker, Vector2 center, float radius,
                                 float baseDamage, DamageType damageType,
                                 LayerMask targetLayer)
    {
        // [PERF] NonAlloc 避免 GC
        int hitCount = Physics2D.OverlapCircleNonAlloc(center, radius, _areaHitBuffer, targetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            var damageable = _areaHitBuffer[i].GetComponent<IDamageable>();
            if (damageable == null || damageable.IsDead) continue;

            // 同一阵营不互伤（攻击者自身）
            if (attacker != null && _areaHitBuffer[i].gameObject == attacker) continue;

            Attack(attacker, damageable, baseDamage, damageType);
        }
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— 环境伤害
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// 施加环境伤害（饥饿/口渴/温度等，无攻击者）。
    /// </summary>
    public void DealEnvironmentalDamage(IDamageable target, float damage, DamageType damageType)
    {
        if (target == null || target.IsDead) return;

        var result = DamageCalculator.CalculateEnvironmental(damage, damageType);

        var damageInfo = new DamageInfo
        {
            Attacker = null,
            Damage = result.FinalDamage,
            DamageType = result.DamageType,
            IsCritical = false,
            KnockbackDirection = Vector2.zero,
            KnockbackForce = 0f
        };

        ApplyDamage(target, damageInfo);
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    /// <summary>施加伤害并广播事件</summary>
    private void ApplyDamage(IDamageable target, DamageInfo info)
    {
        float healthBefore = target.CurrentHealth;
        target.TakeDamage(info);
        float healthAfter = target.CurrentHealth;

        int targetId = target.Transform.gameObject.GetInstanceID();
        int attackerId = info.Attacker != null ? info.Attacker.GetInstanceID() : 0;

        // 广播伤害事件
        EventBus.Publish(new DamageDealtEvent
        {
            SourceInstanceId = attackerId,
            TargetInstanceId = targetId,
            DamageAmount = info.Damage,
            DamageType = info.DamageType,
            IsCritical = info.IsCritical
        });

        // 广播受伤事件（UI用）
        EventBus.Publish(new EntityHitEvent
        {
            EntityInstanceId = targetId,
            DamageAmount = info.Damage,
            RemainingHealth = healthAfter,
            MaxHealth = target.MaxHealth,
            IsCritical = info.IsCritical
        });

        // 广播生命值变化
        EventBus.Publish(new HealthChangedEvent
        {
            EntityInstanceId = targetId,
            OldHealth = healthBefore,
            NewHealth = healthAfter,
            MaxHealth = target.MaxHealth
        });

        // 死亡检测
        if (target.IsDead)
        {
            EventBus.Publish(new EntityDiedEvent
            {
                EntityInstanceId = targetId,
                KillerInstanceId = attackerId,
                Cause = info.Attacker != null ? DeathCause.Combat : DeathCause.Unknown
            });
        }
    }
}
