// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/07_Shared/Extensions/TransformExtensions.cs
// Transform 扩展方法。所有层均可使用。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// Transform 扩展方法集。
/// </summary>
public static class TransformExtensions
{
    /// <summary>销毁所有子物体</summary>
    public static void DestroyAllChildren(this Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(parent.GetChild(i).gameObject);
        }
    }

    /// <summary>重置本地变换</summary>
    public static void ResetLocal(this Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }

    /// <summary>设置 X 坐标（世界坐标）</summary>
    public static void SetPositionX(this Transform t, float x)
    {
        var pos = t.position;
        pos.x = x;
        t.position = pos;
    }

    /// <summary>设置 Y 坐标（世界坐标）</summary>
    public static void SetPositionY(this Transform t, float y)
    {
        var pos = t.position;
        pos.y = y;
        t.position = pos;
    }

    /// <summary>获取 2D 平面上到目标的距离</summary>
    public static float Distance2D(this Transform t, Transform other)
    {
        return Vector2.Distance(t.position, other.position);
    }

    /// <summary>获取 2D 平面上到目标的方向（归一化）</summary>
    public static Vector2 Direction2D(this Transform t, Transform target)
    {
        return ((Vector2)(target.position - t.position)).normalized;
    }

    /// <summary>面向目标（2D，通过翻转 localScale.x）</summary>
    public static void FaceTarget2D(this Transform t, Transform target)
    {
        if (target == null) return;
        float dir = target.position.x - t.position.x;
        if (Mathf.Abs(dir) < 0.01f) return;

        Vector3 scale = t.localScale;
        scale.x = Mathf.Abs(scale.x) * (dir > 0 ? 1 : -1);
        t.localScale = scale;
    }
}
