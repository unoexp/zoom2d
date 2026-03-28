// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/01_Data/ScriptableObjects/Building/BuildingDefinitionSO.cs
// 建筑/庇护所模块数据定义。纯数据，零运行时逻辑。
// ══════════════════════════════════════════════════════════════════════
using System;
using UnityEngine;

/// <summary>
/// 建造放置类型
/// </summary>
public enum BuildPlacementType
{
    Grid        = 0,    // 格子式（墙壁/地板/天花板）
    Free        = 1,    // 自由摆放（家具/装饰/设备）
}

/// <summary>
/// 庇护所模块类别
/// </summary>
public enum ShelterModuleCategory
{
    Structure   = 0,    // 结构（墙壁/地板）
    Functional  = 1,    // 功能模块（供暖炉/制作台/冶炼炉）
    Storage     = 2,    // 储存（储藏室/箱子）
    Utility     = 3,    // 设施（集水装置/通风管道/氧气循环）
    Furniture   = 4,    // 家具（床/椅子/灯）
    Decoration  = 5,    // 装饰
}

/// <summary>
/// 建筑/庇护所模块定义。描述一个可建造物件的数据。
/// 新增建筑只需创建 .asset 文件。
/// </summary>
[CreateAssetMenu(fileName = "Building_", menuName = "SurvivalGame/Building/Building Definition")]
public class BuildingDefinitionSO : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("全局唯一ID")]
    public string BuildingId;

    [Tooltip("显示名称")]
    public string DisplayName;

    [TextArea]
    [Tooltip("描述")]
    public string Description;

    [Tooltip("建筑图标")]
    public Sprite Icon;

    [Tooltip("建筑预制体")]
    public GameObject Prefab;

    [Tooltip("建造预览预制体（半透明占位）")]
    public GameObject PreviewPrefab;

    [Header("建造属性")]
    [Tooltip("放置类型")]
    public BuildPlacementType PlacementType = BuildPlacementType.Grid;

    [Tooltip("庇护所模块类别")]
    public ShelterModuleCategory Category = ShelterModuleCategory.Structure;

    [Tooltip("建造耗时（秒），0=即时")]
    public float BuildDuration = 0f;

    [Tooltip("占用格子大小（仅 Grid 模式）")]
    public Vector2Int GridSize = Vector2Int.one;

    [Header("建造材料")]
    [Tooltip("所需材料列表")]
    public BuildingMaterial[] RequiredMaterials;

    [Header("解锁与前置")]
    [Tooltip("是否默认解锁")]
    public bool UnlockedByDefault = false;

    [Tooltip("前置建筑ID（需要先建造此建筑）")]
    public string PrerequisiteBuildingId;

    [Header("功能效果")]
    [Tooltip("建造后解锁的地层ID（如 L1, L2）")]
    public string UnlocksLayerId;

    [Tooltip("建造后增加的背包容量")]
    public int InventoryCapacityBonus = 0;

    [Tooltip("建造后是否消除庇护所内体温衰减")]
    public bool ProvidesHeating = false;

    [Tooltip("是否提供工作台功能")]
    public bool ProvidesWorkbench = false;
}

/// <summary>
/// 建造材料条目
/// </summary>
[Serializable]
public struct BuildingMaterial
{
    [Tooltip("所需物品")]
    public ItemDefinitionSO Item;

    [Tooltip("所需数量")]
    public int Amount;
}
