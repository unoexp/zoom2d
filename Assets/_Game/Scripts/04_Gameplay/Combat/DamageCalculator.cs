// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Combat/DamageCalculator.cs
// 伤害计算器。纯逻辑，无状态，负责攻防数值计算。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 伤害计算工具类（静态，无状态）。
///
/// 计算流程：
///   基础伤害 → 暴击判定 → 伤害类型抗性 → 最终伤害
///
/// 设计说明：
///   · 纯函数，易于单元测试
///   · 后续可扩展护甲穿透、元素克制等
/// </summary>
public static class DamageCalculator
{
    // ══════════════════════════════════════════════════════
    // 常量
    // ══════════════════════════════════════════════════════

    /// <summary>暴击伤害倍率</summary>
    private const float CRITICAL_MULTIPLIER = 1.5f;

    /// <summary>默认暴击率</summary>
    private const float DEFAULT_CRIT_CHANCE = 0.1f;

    /// <summary>最小伤害（防止 0 伤害）</summary>
    private const float MIN_DAMAGE = 1f;

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// 计算最终伤害。
    /// </summary>
    /// <param name="baseDamage">基础攻击力</param>
    /// <param name="damageType">伤害类型</param>
    /// <param name="defense">目标防御力</param>
    /// <param name="resistance">目标对该伤害类型的抗性（0~1，1=完全免疫）</param>
    /// <param name="critChance">暴击率（0~1）</param>
    /// <returns>计算结果</returns>
    public static DamageResult Calculate(
        float baseDamage,
        DamageType damageType,
        float defense = 0f,
        float resistance = 0f,
        float critChance = DEFAULT_CRIT_CHANCE)
    {
        var result = new DamageResult();

        // 1. 暴击判定
        result.IsCritical = Random.value < critChance;
        float damage = baseDamage;
        if (result.IsCritical)
            damage *= CRITICAL_MULTIPLIER;

        // 2. 减去防御力
        damage -= defense;

        // 3. 伤害类型抗性（真实伤害无视抗性）
        if (damageType != DamageType.True)
        {
            damage *= (1f - Mathf.Clamp01(resistance));
        }

        // 4. 保底伤害
        result.FinalDamage = Mathf.Max(damage, MIN_DAMAGE);
        result.DamageType = damageType;

        return result;
    }

    /// <summary>
    /// 计算环境伤害（饥饿/口渴/温度等，无暴击无抗性）。
    /// </summary>
    public static DamageResult CalculateEnvironmental(float damage, DamageType type)
    {
        return new DamageResult
        {
            FinalDamage = damage,
            DamageType = type,
            IsCritical = false
        };
    }
}

/// <summary>
/// 伤害计算结果。
/// </summary>
public struct DamageResult
{
    public float FinalDamage;
    public DamageType DamageType;
    public bool IsCritical;
}
