
// ─────────────────────────────────────────────────────────────────────
// 📁 Assets/_Game/03_CoreSystems/SurvivalStatus/StatusAttribute.cs
// 运行时单个属性的数值容器，封装 Clamp 边界逻辑
// ─────────────────────────────────────────────────────────────────────
using UnityEngine;

public class StatusAttribute
{
    public SurvivalAttributeType Type       { get; }
    public float                 CurrentValue { get; private set; }
    public float                 MaxValue     { get; private set; }
    public float                 MinValue     { get; private set; }

    public StatusAttribute(
        SurvivalAttributeType type,
        float initialValue,
        float maxValue,
        float minValue = 0f)
    {
        Type         = type;
        MaxValue     = maxValue;
        MinValue     = minValue;
        CurrentValue = Mathf.Clamp(initialValue, MinValue, MaxValue);
    }

    /// <summary>增量修改，自动 Clamp</summary>
    public void ApplyDelta(float delta)
        => CurrentValue = Mathf.Clamp(CurrentValue + delta, MinValue, MaxValue);

    /// <summary>直接赋值（存档读取用），自动 Clamp</summary>
    public void SetValue(float value)
        => CurrentValue = Mathf.Clamp(value, MinValue, MaxValue);

    /// <summary>修改上限（装备/升级影响），同步 Clamp 当前值</summary>
    public void ModifyMax(float delta)
    {
        MaxValue     = Mathf.Max(MinValue, MaxValue + delta);
        CurrentValue = Mathf.Clamp(CurrentValue, MinValue, MaxValue);
    }

    /// <summary>直接设置上限（存档读取用）</summary>
    public void SetMax(float value)
    {
        MaxValue     = Mathf.Max(MinValue, value);
        CurrentValue = Mathf.Clamp(CurrentValue, MinValue, MaxValue);
    }
}
