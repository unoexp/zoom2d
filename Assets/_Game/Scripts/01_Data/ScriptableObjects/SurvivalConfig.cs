// 📁 Assets/_Game/01_Data/ScriptableObjects/SurvivalConfig/SurvivalConfigSO.cs
// ─────────────────────────────────────────────────────────────────────
// 生存属性系统的全局数值配置。
// 所有数值参数均在此 ScriptableObject 中定义，
// 策划可直接在 Inspector 中调整，无需修改任何代码。
// ─────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SurvivalConfig",
    menuName  = "SurvivalGame/Config/Survival Config")]
public class SurvivalConfigSO : ScriptableObject
{
    // ══════════════════════════════════════════════════════
    // 属性初始值配置
    // ══════════════════════════════════════════════════════

    [Header("─── 属性初始值 ───────────────────────────")]
    [Tooltip("定义每种生存属性的初始值、最大值、最小值。\n" +
             "在此列表中添加条目即可扩展新属性，系统自动识别。")]
    public List<AttributeInitEntry> InitialAttributes = new List<AttributeInitEntry>
    {
        // 默认预填，Inspector 中可直接修改
        new AttributeInitEntry(SurvivalAttributeType.Health,      100f, 100f, 0f),
        new AttributeInitEntry(SurvivalAttributeType.Hunger,      100f, 100f, 0f),
        new AttributeInitEntry(SurvivalAttributeType.Thirst,      100f, 100f, 0f),
        new AttributeInitEntry(SurvivalAttributeType.Temperature,  37f,  42f, 0f),
        new AttributeInitEntry(SurvivalAttributeType.Stamina,     100f, 100f, 0f),
        new AttributeInitEntry(SurvivalAttributeType.Sanity,      100f, 100f, 0f),
    };

    // ══════════════════════════════════════════════════════
    // 被动衰减规则
    // ══════════════════════════════════════════════════════

    [Header("─── 被动衰减规则 ─────────────────────────")]
    [Tooltip("每种属性每秒自然衰减量。\n" +
             "EnableDecay = false 可临时关闭某属性的衰减（如调试）。")]
    public List<DecayRuleEntry> PassiveDecayRules = new List<DecayRuleEntry>
    {
        new DecayRuleEntry(SurvivalAttributeType.Hunger,      0.5f,  true),
        new DecayRuleEntry(SurvivalAttributeType.Thirst,      0.8f,  true),  // 口渴衰减更快
        new DecayRuleEntry(SurvivalAttributeType.Stamina,     0f,    false), // 体力由行为驱动，不自然衰减
        new DecayRuleEntry(SurvivalAttributeType.Sanity,      0.1f,  true),
    };
    // 💡 体温不在此列表，由 TemperatureSystem 根据环境动态驱动

    // ══════════════════════════════════════════════════════
    // 致死伤害配置
    // ══════════════════════════════════════════════════════

    [Header("─── 属性归零后的持续伤害(每秒) ───────────")]

    [Tooltip("饥饿值归零后每秒扣除的生命值")]
    [Range(0f, 20f)]
    public float StarvationDamagePerSecond = 1.0f;

    [Tooltip("口渴值归零后每秒扣除的生命值（通常比饥饿更快）")]
    [Range(0f, 20f)]
    public float DehydrationDamagePerSecond = 2.0f;

    [Tooltip("低体温（冻伤）每秒扣除的生命值")]
    [Range(0f, 20f)]
    public float HypotherrmiaDamagePerSecond = 1.5f;

    [Tooltip("高体温（中暑）每秒扣除的生命值")]
    [Range(0f, 20f)]
    public float HyperthermiadamagePerSecond = 1.2f;

    // ══════════════════════════════════════════════════════
    // 体温临界阈值
    // ══════════════════════════════════════════════════════

    [Header("─── 体温临界阈值 ─────────────────────────")]

    [Tooltip("低于此体温值(°C)触发冻伤伤害")]
    [Range(0f, 36f)]
    public float HypothermiaThreshold = 35.0f;

    [Tooltip("高于此体温值(°C)触发中暑伤害")]
    [Range(37f, 45f)]
    public float HyperthermiaThreshold = 40.0f;

    // ══════════════════════════════════════════════════════
    // UI 预警阈值（归一化比例）
    // ══════════════════════════════════════════════════════

    [Header("─── UI 预警阈值 (0~1 归一化比例) ─────────")]
    [Tooltip("饥饿值低于此比例时，HUD 开始闪烁预警")]
    [Range(0f, 1f)]
    public float HungerWarningThreshold = 0.25f;

    [Tooltip("口渴值低于此比例时，HUD 开始闪烁预警")]
    [Range(0f, 1f)]
    public float ThirstWarningThreshold = 0.25f;

    [Tooltip("血量低于此比例时，HUD 开始闪烁预警")]
    [Range(0f, 1f)]
    public float HealthWarningThreshold = 0.30f;

#if UNITY_EDITOR
    // ══════════════════════════════════════════════════════
    // Editor 校验（仅编辑器下运行，不进包）
    // ══════════════════════════════════════════════════════
    private void OnValidate()
    {
        if (HypothermiaThreshold >= HyperthermiaThreshold)
            Debug.LogWarning(
                $"[SurvivalConfigSO] HypothermiaThreshold({HypothermiaThreshold}) " +
                $"应小于 HyperthermiaThreshold({HyperthermiaThreshold})！");

        if (DehydrationDamagePerSecond < StarvationDamagePerSecond)
            Debug.LogWarning(
                "[SurvivalConfigSO] 通常脱水伤害应 >= 饥饿伤害，请确认数值设计意图。");
    }
#endif
}

// ─────────────────────────────────────────────────────────────────────
// 📁 同文件：配套数据结构（嵌套数据类，仅供 SurvivalConfigSO 使用）
// ─────────────────────────────────────────────────────────────────────

/// <summary>
/// 属性初始值条目。
/// 描述一个生存属性在游戏开始时的初始状态。
/// </summary>
[Serializable]
public class AttributeInitEntry
{
    [Tooltip("属性类型")]
    public SurvivalAttributeType AttributeType;

    [Tooltip("游戏开始时的初始值")]
    public float InitialValue;

    [Tooltip("该属性的最大值上限")]
    public float MaxValue;

    [Tooltip("该属性的最小值下限（通常为0）")]
    public float MinValue;

    public AttributeInitEntry() { }

    public AttributeInitEntry(
        SurvivalAttributeType type,
        float initialValue,
        float maxValue,
        float minValue)
    {
        AttributeType = type;
        InitialValue  = initialValue;
        MaxValue      = maxValue;
        MinValue      = minValue;
    }
}

/// <summary>
/// 被动衰减规则条目。
/// 描述一个属性每秒自然衰减的速率及开关。
/// </summary>
[Serializable]
public class DecayRuleEntry
{
    [Tooltip("目标属性类型")]
    public SurvivalAttributeType AttributeType;

    [Tooltip("每秒衰减量（正数=减少）")]
    [Range(0f, 10f)]
    public float DecayRatePerSecond;

    [Tooltip("是否启用此属性的自然衰减")]
    public bool EnableDecay;

    public DecayRuleEntry() { }

    public DecayRuleEntry(
        SurvivalAttributeType type,
        float decayRatePerSecond,
        bool  enableDecay)
    {
        AttributeType       = type;
        DecayRatePerSecond  = decayRatePerSecond;
        EnableDecay         = enableDecay;
    }
}

