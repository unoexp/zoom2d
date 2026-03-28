// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/07_Shared/Extensions/CollectionExtensions.cs
// 集合类扩展方法。所有层均可使用。
// ══════════════════════════════════════════════════════════════════════
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 集合类扩展方法集。
/// </summary>
public static class CollectionExtensions
{
    /// <summary>判断列表是否为空或 null</summary>
    public static bool IsNullOrEmpty<T>(this IList<T> list)
    {
        return list == null || list.Count == 0;
    }

    /// <summary>安全获取列表元素，越界返回默认值</summary>
    public static T GetSafe<T>(this IList<T> list, int index, T defaultValue = default)
    {
        if (list == null || index < 0 || index >= list.Count)
            return defaultValue;
        return list[index];
    }

    /// <summary>从列表中随机取一个元素</summary>
    public static T GetRandom<T>(this IList<T> list)
    {
        if (list == null || list.Count == 0) return default;
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>Fisher-Yates 原地洗牌</summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        if (list == null) return;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    /// <summary>安全获取字典值，不存在返回默认值（避免 TryGetValue 的两行写法）</summary>
    public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict,
        TKey key, TValue defaultValue = default)
    {
        if (dict != null && dict.TryGetValue(key, out var value))
            return value;
        return defaultValue;
    }
}
