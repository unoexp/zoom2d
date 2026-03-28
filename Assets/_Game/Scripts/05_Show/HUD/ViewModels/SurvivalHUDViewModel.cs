// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/HUD/ViewModels/SurvivalHUDViewModel.cs
// 生存状态HUD的ViewModel，持有UI展示数据。
// ══════════════════════════════════════════════════════════════════════
using System;

/// <summary>
/// 单条生存属性的UI展示数据
/// </summary>
public struct AttributeDisplayData
{
    /// <summary>属性类型</summary>
    public SurvivalAttributeType Type;
    /// <summary>当前值</summary>
    public float CurrentValue;
    /// <summary>最大值</summary>
    public float MaxValue;
    /// <summary>归一化比例 [0,1]</summary>
    public float Normalized;
    /// <summary>预警等级（-1=无预警）</summary>
    public int WarningLevel;
}

/// <summary>
/// 生存状态HUD的ViewModel。
///
/// 纯C#类，持有UI状态数据。
/// 暴露事件给View订阅，Presenter写入数据时触发更新。
/// </summary>
public class SurvivalHUDViewModel
{
    // ══════════════════════════════════════════════════════
    // 事件（View订阅）
    // ══════════════════════════════════════════════════════

    /// <summary>属性数据更新</summary>
    public event Action<AttributeDisplayData> OnAttributeUpdated;

    /// <summary>预警状态变化</summary>
    public event Action<SurvivalAttributeType, CriticalWarningLevel> OnWarningChanged;

    /// <summary>消耗品使用反馈</summary>
    public event Action<string> OnConsumableFeedback;

    // ══════════════════════════════════════════════════════
    // 数据（缓存当前显示值）
    // ══════════════════════════════════════════════════════

    private readonly AttributeDisplayData[] _attributes;
    private const int ATTRIBUTE_COUNT = 5; // Health, Hunger, Thirst, Stamina, Temperature

    // 属性类型到内部索引的映射
    private static readonly SurvivalAttributeType[] _trackedTypes = new SurvivalAttributeType[]
    {
        SurvivalAttributeType.Health,
        SurvivalAttributeType.Hunger,
        SurvivalAttributeType.Thirst,
        SurvivalAttributeType.Stamina,
        SurvivalAttributeType.Temperature,
    };

    // ══════════════════════════════════════════════════════
    // 构造
    // ══════════════════════════════════════════════════════

    public SurvivalHUDViewModel()
    {
        _attributes = new AttributeDisplayData[ATTRIBUTE_COUNT];
        for (int i = 0; i < ATTRIBUTE_COUNT; i++)
        {
            _attributes[i] = new AttributeDisplayData
            {
                Type = _trackedTypes[i],
                CurrentValue = 0f,
                MaxValue = 100f,
                Normalized = 0f,
                WarningLevel = -1
            };
        }
    }

    // ══════════════════════════════════════════════════════
    // 公有 API（Presenter调用）
    // ══════════════════════════════════════════════════════

    /// <summary>更新属性数据</summary>
    public void UpdateAttribute(SurvivalAttributeType type, float current, float max)
    {
        int index = GetIndex(type);
        if (index < 0) return;

        float normalized = max > 0f ? current / max : 0f;
        if (normalized < 0f) normalized = 0f;
        if (normalized > 1f) normalized = 1f;

        _attributes[index] = new AttributeDisplayData
        {
            Type = type,
            CurrentValue = current,
            MaxValue = max,
            Normalized = normalized,
            WarningLevel = _attributes[index].WarningLevel
        };

        OnAttributeUpdated?.Invoke(_attributes[index]);
    }

    /// <summary>更新预警等级</summary>
    public void UpdateWarning(SurvivalAttributeType type, CriticalWarningLevel level)
    {
        int index = GetIndex(type);
        if (index < 0) return;

        _attributes[index].WarningLevel = (int)level;
        OnWarningChanged?.Invoke(type, level);
    }

    /// <summary>显示消耗品使用反馈</summary>
    public void ShowConsumableFeedback(string displayName)
    {
        OnConsumableFeedback?.Invoke(displayName);
    }

    /// <summary>获取属性数据</summary>
    public AttributeDisplayData GetAttribute(SurvivalAttributeType type)
    {
        int index = GetIndex(type);
        return index >= 0 ? _attributes[index] : default;
    }

    /// <summary>获取所有被跟踪的属性类型</summary>
    public static SurvivalAttributeType[] TrackedTypes => _trackedTypes;

    // ══════════════════════════════════════════════════════
    // 内部
    // ══════════════════════════════════════════════════════

    /// <summary>获取属性类型在内部数组中的索引</summary>
    private int GetIndex(SurvivalAttributeType type)
    {
        // [PERF] 线性查找，仅5个元素，无需字典
        for (int i = 0; i < ATTRIBUTE_COUNT; i++)
        {
            if (_trackedTypes[i] == type) return i;
        }
        return -1;
    }
}
