// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/01_Data/ScriptableObjects/Quest/QuestDefinitionSO.cs
// 任务数据定义。纯数据，零运行时逻辑。
// 💡 新增任务只需创建 .asset 文件，无需改代码。
// ══════════════════════════════════════════════════════════════════════
using System;
using UnityEngine;

/// <summary>
/// 任务目标类型
/// </summary>
public enum QuestObjectiveType
{
    CollectItem     = 0,    // 收集指定物品
    CraftItem       = 1,    // 制作指定物品
    BuildStructure  = 2,    // 建造指定建筑
    ReachLayer      = 3,    // 到达指定地层深度
    DefeatEnemy     = 4,    // 击败指定类型敌人
    TalkToNPC       = 5,    // 与指定NPC对话
    SurviveDays     = 6,    // 存活指定天数
    ExploreArea     = 7,    // 探索指定区域
}

/// <summary>
/// 任务目标条目。描述一个任务目标的完成条件。
/// </summary>
[Serializable]
public struct QuestObjective
{
    [Tooltip("目标类型")]
    public QuestObjectiveType Type;

    [Tooltip("目标描述（UI显示）")]
    public string Description;

    [Tooltip("目标ID（物品ID/建筑ID/敌人类型/NPC ID/地层ID）")]
    public string TargetId;

    [Tooltip("需要的数量")]
    public int RequiredAmount;

    [Tooltip("是否为可选目标")]
    public bool IsOptional;
}

/// <summary>
/// 任务奖励条目
/// </summary>
[Serializable]
public struct QuestReward
{
    [Tooltip("奖励物品")]
    public ItemDefinitionSO Item;

    [Tooltip("奖励数量")]
    public int Amount;
}

/// <summary>
/// 任务定义 ScriptableObject。
///
/// 核心职责：
///   · 定义任务的基础信息（名称、描述、图标）
///   · 定义任务目标列表（可多个目标）
///   · 定义任务奖励
///   · 定义任务的前置条件和触发条件
///
/// 设计说明：
///   · 数据驱动，新增任务只需创建 .asset 文件
///   · 支持主线任务和支线任务
///   · 目标支持多种类型（收集/制作/建造/击杀等）
/// </summary>
[CreateAssetMenu(fileName = "Quest_", menuName = "SurvivalGame/Quest/Quest Definition")]
public class QuestDefinitionSO : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("任务唯一ID")]
    public string QuestId;

    [Tooltip("任务显示名称")]
    public string DisplayName;

    [TextArea]
    [Tooltip("任务描述")]
    public string Description;

    [Tooltip("任务图标")]
    public Sprite Icon;

    [Header("任务类型")]
    [Tooltip("是否为主线任务")]
    public bool IsMainQuest = false;

    [Tooltip("任务章节（主线任务用，1-6）")]
    public int Chapter = 1;

    [Header("任务目标")]
    [Tooltip("任务目标列表")]
    public QuestObjective[] Objectives;

    [Header("奖励")]
    [Tooltip("完成奖励")]
    public QuestReward[] Rewards;

    [Header("条件")]
    [Tooltip("前置任务ID（需先完成此任务）")]
    public string PrerequisiteQuestId;

    [Tooltip("所需庇护所阶段")]
    public int RequiredShelterStage = 0;

    [Tooltip("是否自动接取（满足条件时自动激活）")]
    public bool AutoAccept = false;

    [Tooltip("发起NPC的ID（空字符串 = 系统任务）")]
    public string GiverNPCId;
}
