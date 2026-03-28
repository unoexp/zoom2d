// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/01_Data/ScriptableObjects/Audio/AudioCatalogSO.cs
// 音效目录数据定义。将字符串ID映射到AudioClip，支持分组和随机变体。
// 💡 新增音效只需在 Inspector 中添加条目，无需改代码。
// ══════════════════════════════════════════════════════════════════════
using System;
using UnityEngine;

/// <summary>
/// 音效条目。定义一个音效ID对应的AudioClip和播放参数。
/// </summary>
[Serializable]
public struct AudioEntry
{
    [Tooltip("音效唯一ID（如 sfx_pickup, sfx_hit_melee, bgm_main_menu）")]
    public string AudioId;

    [Tooltip("音频分组")]
    public AudioGroup Group;

    [Tooltip("音频剪辑（多个时随机选择一个播放，增加音效变化）")]
    public AudioClip[] Clips;

    [Tooltip("音量缩放（0-2，1=正常）")]
    [Range(0f, 2f)]
    public float VolumeScale;

    [Tooltip("音高范围最小值（随机音高变化）")]
    [Range(0.5f, 2f)]
    public float PitchMin;

    [Tooltip("音高范围最大值")]
    [Range(0.5f, 2f)]
    public float PitchMax;
}

/// <summary>
/// 音效目录 ScriptableObject。
///
/// 核心职责：
///   · 集中管理所有音效的 ID → AudioClip 映射
///   · 支持同一ID多个变体（随机选择）
///   · 支持音量缩放和音高随机范围
///   · 通过 Inspector 配置，数据驱动
///
/// 设计说明：
///   · 可创建多个目录实例分类管理（SFX目录、BGM目录、环境音目录等）
///   · AudioManager 在初始化时加载并合并所有目录
///   · 纯数据，零运行时逻辑
/// </summary>
[CreateAssetMenu(fileName = "AudioCatalog_", menuName = "SurvivalGame/Audio/Audio Catalog")]
public class AudioCatalogSO : ScriptableObject
{
    [Header("音效条目列表")]
    public AudioEntry[] Entries;

    /// <summary>条目数量</summary>
    public int Count => Entries != null ? Entries.Length : 0;
}
