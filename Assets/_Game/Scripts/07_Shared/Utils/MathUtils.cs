// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/07_Shared/Utils/MathUtils.cs
// 数学工具方法。纯静态，零依赖。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 数学工具方法集。
/// </summary>
public static class MathUtils
{
    /// <summary>将值映射到新的范围</summary>
    public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        if (Mathf.Approximately(fromMax, fromMin)) return toMin;
        return toMin + (value - fromMin) / (fromMax - fromMin) * (toMax - toMin);
    }

    /// <summary>带阻尼的平滑趋近（每帧调用）</summary>
    public static float SmoothDamp01(float current, float target, float speed, float deltaTime)
    {
        return Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * deltaTime));
    }

    /// <summary>判断两个浮点数是否近似相等</summary>
    public static bool Approximately(float a, float b, float epsilon = 0.001f)
    {
        return Mathf.Abs(a - b) < epsilon;
    }

    /// <summary>计算百分比文本（如 "75%"）</summary>
    public static string ToPercentString(float normalized)
    {
        return $"{Mathf.RoundToInt(Mathf.Clamp01(normalized) * 100f)}%";
    }

    /// <summary>环绕 Clamp（如角度 0~360）</summary>
    public static float WrapClamp(float value, float min, float max)
    {
        float range = max - min;
        if (range <= 0f) return min;
        value = ((value - min) % range + range) % range + min;
        return value;
    }

    /// <summary>二维距离的平方（避免开方，用于距离比较）</summary>
    public static float SqrDistance2D(Vector2 a, Vector2 b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return dx * dx + dy * dy;
    }
}
