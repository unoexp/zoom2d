// 📁 01_Data/ScriptableObjects/Items/Consumable/ConsumableItemSO.cs
// 消耗品物品数据定义（食物、饮水、药品等）
using UnityEngine;

/// <summary>
/// 消耗品效果类型
/// </summary>
public enum ConsumableEffectType
{
    RestoreHealth       = 0,    // 恢复生命值
    RestoreHunger       = 1,    // 恢复饱食度
    RestoreThirst       = 2,    // 恢复水分
    RestoreStamina      = 3,    // 恢复体力
    RestoreTemperature  = 4,    // 恢复体温
    Buff                = 10,   // 施加增益效果
    Debuff              = 11,   // 施加减益效果（食物副作用）
    CureEffect          = 20,   // 解除状态效果
}

/// <summary>
/// 单条消耗品效果定义
/// </summary>
[System.Serializable]
public struct ConsumableEffect
{
    [Tooltip("效果类型")]
    public ConsumableEffectType EffectType;

    [Tooltip("对应的生存属性类型（仅 Restore* 类型使用）")]
    public SurvivalAttributeType AttributeType;

    [Tooltip("效果数值（正=恢复，负=消耗/伤害）")]
    public float Value;

    [Tooltip("是否为持续效果（true=每秒生效，false=瞬时）")]
    public bool IsOverTime;

    [Tooltip("持续时间（秒），仅 IsOverTime=true 时有效")]
    public float Duration;

    [Tooltip("要施加/解除的状态效果ID（仅 Buff/Debuff/CureEffect 使用）")]
    public string StatusEffectId;
}

/// <summary>
/// 消耗品物品定义：食物、饮水、药品等可使用物品。
/// 支持多效果组合（如一碗热汤同时恢复饥饿+体温）。
/// 新增消耗品只需创建.asset文件，无需改代码。
/// </summary>
[CreateAssetMenu(fileName = "Item_Consumable_", menuName = "SurvivalGame/Items/Consumable")]
public class ConsumableItemSO : ItemDefinitionSO
{
    [Header("消耗品属性")]
    [Tooltip("使用动画时间（秒），0=瞬时使用")]
    public float UseTime = 1f;

    [Tooltip("使用后的冷却时间（秒）")]
    public float Cooldown = 0f;

    [Tooltip("使用时播放的音效ID")]
    public string UseSoundId;

    [Header("效果列表")]
    [Tooltip("该消耗品的所有效果（支持多效果组合）")]
    public ConsumableEffect[] Effects;
}
