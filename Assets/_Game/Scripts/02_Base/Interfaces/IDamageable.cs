// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Base/Interfaces/IDamageable.cs
// 可受伤害接口。所有可被攻击的实体（玩家、敌人、可破坏物）实现此接口。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 可受伤害接口。
///
/// 使用方式：
///   · 挂载在可被攻击的 GameObject 上
///   · CombatSystem / DamageCalculator 通过此接口施加伤害
///   · 🏗️ 定义在 02_Base，04_Gameplay 实现
/// </summary>
public interface IDamageable
{
    /// <summary>当前生命值</summary>
    float CurrentHealth { get; }

    /// <summary>最大生命值</summary>
    float MaxHealth { get; }

    /// <summary>是否已死亡</summary>
    bool IsDead { get; }

    /// <summary>实体所属的 GameObject</summary>
    Transform Transform { get; }

    /// <summary>
    /// 接受伤害。
    /// </summary>
    /// <param name="info">伤害信息</param>
    void TakeDamage(DamageInfo info);

    /// <summary>
    /// 接受治疗。
    /// </summary>
    /// <param name="amount">治疗量</param>
    void Heal(float amount);
}

/// <summary>
/// 伤害信息结构体。在伤害流程中传递，包含计算后的最终伤害。
/// </summary>
[System.Serializable]
public struct DamageInfo
{
    /// <summary>攻击者 GameObject（null = 环境伤害）</summary>
    public GameObject Attacker;

    /// <summary>最终伤害值（经过 DamageCalculator 计算后）</summary>
    public float Damage;

    /// <summary>伤害类型</summary>
    public DamageType DamageType;

    /// <summary>是否暴击</summary>
    public bool IsCritical;

    /// <summary>击退方向（归一化）</summary>
    public Vector2 KnockbackDirection;

    /// <summary>击退力度</summary>
    public float KnockbackForce;
}
