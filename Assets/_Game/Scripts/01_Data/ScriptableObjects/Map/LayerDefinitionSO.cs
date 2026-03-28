// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/01_Data/ScriptableObjects/Map/LayerDefinitionSO.cs
// 地层定义数据。描述一个地层的硬度、深度、产出等参数。
// ══════════════════════════════════════════════════════════════════════
using System;
using UnityEngine;

/// <summary>
/// 地层定义数据。对应 GDD 7.2 地层参数配置表。
/// 新增地层只需创建 .asset 文件。
/// </summary>
[CreateAssetMenu(fileName = "Layer_", menuName = "SurvivalGame/Map/Layer Definition")]
public class LayerDefinitionSO : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("地层ID（如 S, L1, L2...）")]
    public string LayerId;

    [Tooltip("显示名称")]
    public string DisplayName;

    [Tooltip("地层深度（米，负值表示地下）")]
    public float Depth;

    [Header("挖掘参数")]
    [Tooltip("硬度系数（越高越难挖）")]
    public float Hardness = 1f;

    [Tooltip("每格体力消耗")]
    public float StaminaCostPerTile = 8f;

    [Header("产出")]
    [Tooltip("挖掘产出物品列表")]
    public LayerDropEntry[] Drops;

    [Header("解锁条件")]
    [Tooltip("需要的建筑ID（空=初始解锁）")]
    public string RequiredBuildingId;

    [Tooltip("是否默认解锁")]
    public bool UnlockedByDefault = false;

    [Header("环境")]
    [Tooltip("地层环境温度修正")]
    public float TemperatureModifier = 0f;

    [Tooltip("是否需要氧气支持")]
    public bool RequiresOxygen = false;
}

/// <summary>
/// 地层挖掘产出条目
/// </summary>
[Serializable]
public struct LayerDropEntry
{
    [Tooltip("产出物品")]
    public ItemDefinitionSO Item;

    [Tooltip("最少数量")]
    public int MinAmount;

    [Tooltip("最多数量")]
    public int MaxAmount;

    [Tooltip("掉落概率 0~1")]
    [Range(0f, 1f)]
    public float DropChance;
}
