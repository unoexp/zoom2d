// ══════════════════════════════════════════════════════════════════════
// 📁 03_Core/Save/SaveSerializer.cs
// 存档序列化器，将 ISaveable 采集的状态数据转换为 JSON 字符串
// ══════════════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档序列化工具类。
///
/// 将各 ISaveable 系统的状态数据统一序列化为 JSON 字符串，
/// 使用 <see cref="GameSaveData"/> 作为顶层容器。
///
/// 设计说明：
///   UnityEngine.JsonUtility 不支持直接序列化 Dictionary，
///   因此将每个 ISaveable 的数据单独序列化为 JSON 字符串，
///   以 SaveEntry（key + jsonData）的形式存入 GameSaveData.entries 列表。
/// </summary>
public static class SaveSerializer
{
    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// 将所有 ISaveable 的状态数据序列化为完整的存档 JSON 字符串。
    /// </summary>
    /// <param name="stateMap">SaveKey → CaptureState() 返回的数据对象</param>
    /// <returns>完整存档的 JSON 字符串</returns>
    public static string Serialize(Dictionary<string, object> stateMap)
    {
        var saveData = new GameSaveData
        {
            version  = "1.0",
            saveTime = DateTime.Now.ToString("O") // ISO 8601 格式
        };

        foreach (var kvp in stateMap)
        {
            var entry = new SaveEntry
            {
                key      = kvp.Key,
                jsonData = JsonUtility.ToJson(kvp.Value)
            };
            saveData.entries.Add(entry);
        }

        return JsonUtility.ToJson(saveData, true); // prettyPrint 便于调试
    }

    /// <summary>
    /// 将存档 JSON 字符串反序列化为 SaveKey → JSON字符串 的字典。
    /// 注意：返回的 value 仍是 JSON 字符串，需要各 ISaveable 在 RestoreState 中自行解析。
    /// </summary>
    /// <param name="json">完整存档的 JSON 字符串</param>
    /// <returns>SaveKey → 该系统数据的 JSON 字符串</returns>
    public static Dictionary<string, string> Deserialize(string json)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[SaveSerializer] 反序列化失败：输入 JSON 为空");
            return result;
        }

        var saveData = JsonUtility.FromJson<GameSaveData>(json);
        if (saveData == null)
        {
            Debug.LogError("[SaveSerializer] 反序列化失败：JSON 格式无效");
            return result;
        }

        if (saveData.entries == null) return result;

        // [PERF] 预分配字典容量
        result = new Dictionary<string, string>(saveData.entries.Count);

        for (int i = 0; i < saveData.entries.Count; i++)
        {
            var entry = saveData.entries[i];
            if (string.IsNullOrEmpty(entry.key))
            {
                Debug.LogWarning($"[SaveSerializer] 跳过空 key 的存档条目（索引 {i}）");
                continue;
            }

            result[entry.key] = entry.jsonData;
        }

        return result;
    }
}
