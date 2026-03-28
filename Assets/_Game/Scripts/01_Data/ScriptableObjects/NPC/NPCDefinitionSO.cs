// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/01_Data/ScriptableObjects/NPC/NPCDefinitionSO.cs
// NPC 数据定义。纯数据，零运行时逻辑。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// NPC 行为倾向
/// </summary>
public enum NPCDisposition
{
    Friendly    = 0,    // 友善（可直接对话交易）
    Neutral     = 1,    // 中立（需建立信任）
    Hostile     = 2,    // 敌对（需解除敌意才可交互）
}

/// <summary>
/// NPC 数据定义。描述一个 NPC 的基础属性和行为参数。
/// </summary>
[CreateAssetMenu(fileName = "NPC_", menuName = "SurvivalGame/NPC/NPC Definition")]
public class NPCDefinitionSO : ScriptableObject
{
    [Header("基础信息")]
    public string NPCId;
    public string DisplayName;
    [TextArea] public string Description;
    public Sprite Portrait;
    public GameObject Prefab;

    [Header("行为")]
    [Tooltip("默认行为倾向")]
    public NPCDisposition DefaultDisposition = NPCDisposition.Friendly;

    [Tooltip("信任度阈值（达到此值变为友善）")]
    [Range(0, 100)]
    public int TrustThreshold = 50;

    [Header("对话")]
    [Tooltip("默认对话数据")]
    public DialogDataSO DefaultDialog;

    [Header("交易")]
    [Tooltip("是否可交易")]
    public bool CanTrade = false;

    [Tooltip("出现条件：所需庇护所阶段")]
    public int RequiredShelterStage = 0;

    [Tooltip("出现条件：所需游戏天数")]
    public int RequiredDayCount = 0;
}
