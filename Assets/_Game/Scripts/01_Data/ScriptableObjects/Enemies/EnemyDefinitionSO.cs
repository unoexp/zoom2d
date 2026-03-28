// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/01_Data/ScriptableObjects/Enemies/EnemyDefinitionSO.cs
// 敌人数据定义。纯数据，零运行时逻辑。
// 💡 新增敌人类型只需创建 .asset 文件，无需改代码。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 敌人定义数据。描述一种敌人的基础属性、战斗参数和行为参数。
/// </summary>
[CreateAssetMenu(fileName = "Enemy_", menuName = "SurvivalGame/Enemies/Enemy Definition")]
public class EnemyDefinitionSO : ScriptableObject
{
    // ══════════════════════════════════════════════════════
    // 基础信息
    // ══════════════════════════════════════════════════════

    [Header("基础信息")]
    [Tooltip("全局唯一ID（用于存档/事件/刷新规则引用）")]
    public string EnemyId;

    [Tooltip("显示名称")]
    public string DisplayName;

    [TextArea]
    [Tooltip("描述")]
    public string Description;

    [Tooltip("敌人预制体")]
    public GameObject Prefab;

    [Tooltip("UI头像图标")]
    public Sprite Icon;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    [Header("属性")]
    [Tooltip("最大生命值")]
    public float MaxHealth = 100f;

    [Tooltip("移动速度")]
    public float MoveSpeed = 3f;

    // ══════════════════════════════════════════════════════
    // 战斗参数
    // ══════════════════════════════════════════════════════

    [Header("战斗参数")]
    [Tooltip("基础攻击伤害")]
    public float AttackDamage = 10f;

    [Tooltip("攻击间隔（秒）")]
    public float AttackCooldown = 1.5f;

    [Tooltip("攻击范围")]
    public float AttackRange = 1.5f;

    [Tooltip("伤害类型")]
    public DamageType DamageType = DamageType.Physical;

    // ══════════════════════════════════════════════════════
    // AI 行为参数
    // ══════════════════════════════════════════════════════

    [Header("AI 行为")]
    [Tooltip("视野距离（发现玩家的感知范围）")]
    public float DetectionRange = 8f;

    [Tooltip("追击距离（超过此距离放弃追击）")]
    public float ChaseRange = 15f;

    [Tooltip("逃跑血量阈值（归一化 0~1，低于此比例逃跑）")]
    [Range(0f, 1f)]
    public float FleeHealthThreshold = 0.2f;

    [Tooltip("巡逻速度倍率（相对于 MoveSpeed）")]
    [Range(0.1f, 1f)]
    public float PatrolSpeedMultiplier = 0.5f;

    // ══════════════════════════════════════════════════════
    // 掉落与经验
    // ══════════════════════════════════════════════════════

    [Header("掉落与奖励")]
    [Tooltip("击杀后掉落的物品列表")]
    public EnemyDropEntry[] Drops;

    [Tooltip("击杀经验值")]
    public int ExperienceReward = 10;
}

/// <summary>
/// 敌人掉落条目。描述一种掉落物品及其概率。
/// </summary>
[System.Serializable]
public struct EnemyDropEntry
{
    [Tooltip("掉落物品")]
    public ItemDefinitionSO Item;

    [Tooltip("掉落概率 (0~1)")]
    [Range(0f, 1f)]
    public float DropChance;

    [Tooltip("最少掉落数量")]
    public int MinAmount;

    [Tooltip("最多掉落数量")]
    public int MaxAmount;
}
