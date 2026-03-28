// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/01_Data/ScriptableObjects/Crafting/RecipeDefinitionSO.cs
// 制作配方数据定义。纯数据，零运行时逻辑。
// 💡 新增配方只需创建 .asset 文件，无需改代码。
// ══════════════════════════════════════════════════════════════════════
using System;
using UnityEngine;

/// <summary>
/// 制作配方数据。定义一个配方所需的材料、产出物品及制作条件。
/// </summary>
[CreateAssetMenu(fileName = "Recipe_", menuName = "SurvivalGame/Crafting/Recipe")]
public class RecipeDefinitionSO : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("配方唯一ID，用于存档和事件")]
    public string RecipeId;

    [Tooltip("配方显示名称")]
    public string DisplayName;

    [TextArea]
    [Tooltip("配方描述")]
    public string Description;

    [Tooltip("配方图标（UI用）")]
    public Sprite Icon;

    [Header("制作材料")]
    [Tooltip("所需材料列表")]
    public CraftingIngredient[] Ingredients;

    [Header("产出")]
    [Tooltip("产出物品")]
    public ItemDefinitionSO OutputItem;

    [Tooltip("产出数量")]
    public int OutputAmount = 1;

    [Header("制作条件")]
    [Tooltip("是否需要工作台")]
    public bool RequiresWorkbench = false;

    [Tooltip("制作耗时（秒）。0 = 即时制作")]
    public float CraftingDuration = 0f;

    [Tooltip("是否默认解锁")]
    public bool UnlockedByDefault = true;

    [Tooltip("所需玩家等级（0=无限制）")]
    public int RequiredLevel = 0;
}

/// <summary>
/// 制作材料条目。定义一种所需材料及其数量。
/// </summary>
[Serializable]
public struct CraftingIngredient
{
    [Tooltip("所需物品")]
    public ItemDefinitionSO Item;

    [Tooltip("所需数量")]
    public int Amount;
}
