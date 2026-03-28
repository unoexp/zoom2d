// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/07_Shared/Utils/RandomUtils.cs
// 随机数工具方法。纯静态，零依赖。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 随机数工具方法集。
/// </summary>
public static class RandomUtils
{
    /// <summary>以指定概率返回 true（0~1）</summary>
    public static bool Chance(float probability)
    {
        return Random.value < probability;
    }

    /// <summary>返回随机的 +1 或 -1</summary>
    public static int RandomSign()
    {
        return Random.value < 0.5f ? -1 : 1;
    }

    /// <summary>在圆内随机一个 2D 点（用于巡逻/游荡目标）</summary>
    public static Vector2 RandomPointInCircle(Vector2 center, float radius)
    {
        return center + Random.insideUnitCircle * radius;
    }

    /// <summary>在线段上随机一个点</summary>
    public static Vector2 RandomPointOnLine(Vector2 a, Vector2 b)
    {
        return Vector2.Lerp(a, b, Random.value);
    }

    /// <summary>加权随机选择索引</summary>
    /// <param name="weights">每个选项的权重数组</param>
    /// <returns>被选中的索引</returns>
    public static int WeightedRandom(float[] weights)
    {
        if (weights == null || weights.Length == 0) return -1;

        float totalWeight = 0f;
        for (int i = 0; i < weights.Length; i++)
            totalWeight += weights[i];

        if (totalWeight <= 0f) return 0;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative) return i;
        }

        return weights.Length - 1;
    }

    /// <summary>在范围内随机一个整数（包含 min 和 max）</summary>
    public static int RangeInclusive(int min, int max)
    {
        return Random.Range(min, max + 1);
    }
}
