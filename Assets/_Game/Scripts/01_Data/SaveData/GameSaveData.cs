// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/01_Data/SaveData/GameSaveData.cs
// 存档数据容器，用于 JSON 序列化
// ══════════════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;

/// <summary>
/// 顶层存档数据结构。
/// 包含版本号、存档时间、所有子系统的序列化数据。
/// </summary>
[Serializable]
public class GameSaveData
{
    /// <summary>存档格式版本（用于向前兼容）</summary>
    public string version = "1.0";

    /// <summary>存档时间（ISO 8601 格式）</summary>
    public string saveTime;

    /// <summary>所有子系统的存档条目</summary>
    public List<SaveEntry> entries = new List<SaveEntry>();
}

/// <summary>
/// 单个子系统的存档条目。
/// key 对应 ISaveable.SaveKey，jsonData 是该系统 CaptureState() 的 JSON 序列化结果。
/// </summary>
[Serializable]
public class SaveEntry
{
    /// <summary>系统标识（对应 ISaveable.SaveKey）</summary>
    public string key;

    /// <summary>该系统状态的 JSON 数据</summary>
    public string jsonData;
}
