// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/01_Data/ScriptableObjects/Difficulty/DifficultyPresetSO.cs
// 难度预设数据。对应 GDD 4.3 三档难度系统。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 难度预设数据。定义一档难度下的所有参数修正值。
/// 对应 GDD 4.3 三档预设参数对照表。
/// </summary>
[CreateAssetMenu(fileName = "Difficulty_", menuName = "SurvivalGame/Difficulty/Difficulty Preset")]
public class DifficultyPresetSO : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("预设ID（Easy/Normal/Hard/Custom）")]
    public string PresetId;

    [Tooltip("显示名称")]
    public string DisplayName;

    [TextArea]
    [Tooltip("难度描述")]
    public string Description;

    [Header("全局难度系数")]
    [Tooltip("全局难度倍率")]
    public float DifficultyMultiplier = 1f;

    [Header("生存属性衰减")]
    [Tooltip("饥饿衰减速率（点/分钟）")]
    public float HungerDecayRate = 2f;

    [Tooltip("口渴衰减速率（点/分钟）")]
    public float ThirstDecayRate = 3f;

    [Tooltip("体温衰减速率（点/分钟）")]
    public float WarmthDecayRate = 4f;

    [Tooltip("体力恢复速率（点/分钟）")]
    public float StaminaRecoverRate = 5f;

    [Tooltip("庇护所内衰减修正倍率")]
    [Range(0.1f, 0.8f)]
    public float ShelterDecayModifier = 0.3f;

    [Header("死亡与存档")]
    [Tooltip("是否启用死亡惩罚（读档回退）")]
    public bool DeathPenaltyEnabled = true;

    [Tooltip("是否允许庇护所外存档")]
    public bool OutdoorSaveEnabled = false;

    [Header("战斗")]
    [Tooltip("敌人伤害倍率")]
    public float EnemyDamageMultiplier = 1f;

    [Tooltip("敌人生命值倍率")]
    public float EnemyHealthMultiplier = 1f;
}
