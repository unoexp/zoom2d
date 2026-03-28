// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/01_Data/ScriptableObjects/Dialog/DialogDataSO.cs
// 对话数据定义。支持分支对话和条件触发。
// ══════════════════════════════════════════════════════════════════════
using System;
using UnityEngine;

/// <summary>
/// 对话数据定义。包含一段对话的所有节点。
/// 新增对话只需创建 .asset 文件。
/// </summary>
[CreateAssetMenu(fileName = "Dialog_", menuName = "SurvivalGame/Dialog/Dialog Data")]
public class DialogDataSO : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("对话ID")]
    public string DialogId;

    [Tooltip("对话节点列表（按顺序播放，分支通过 NextNodeIndex 跳转）")]
    public DialogNode[] Nodes;
}

/// <summary>
/// 单个对话节点
/// </summary>
[Serializable]
public struct DialogNode
{
    [Tooltip("说话者名称（空=旁白）")]
    public string SpeakerName;

    [Tooltip("说话者头像")]
    public Sprite SpeakerPortrait;

    [TextArea(2, 5)]
    [Tooltip("对话内容")]
    public string Content;

    [Tooltip("玩家选择项（空=自动进入下一节点）")]
    public DialogChoice[] Choices;

    [Tooltip("无选择项时的下一节点索引（-1=对话结束）")]
    public int NextNodeIndex;

    [Tooltip("到达此节点时触发的事件ID")]
    public string TriggerEventId;
}

/// <summary>
/// 对话选择项
/// </summary>
[Serializable]
public struct DialogChoice
{
    [Tooltip("选项文本")]
    public string Text;

    [Tooltip("选择后跳转的节点索引（-1=对话结束）")]
    public int TargetNodeIndex;

    [Tooltip("信任度变化（正=增加，负=减少）")]
    public int TrustDelta;

    [Tooltip("选项显示条件：所需物品ID（空=无条件）")]
    public string RequiredItemId;
}
