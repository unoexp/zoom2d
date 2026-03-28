// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/03_Core/Difficulty/DifficultySystem.cs
// 难度系统。管理当前难度预设，提供参数查询接口。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 难度管理系统。
///
/// 核心职责：
///   · 管理当前选择的难度预设
///   · 提供各系统查询难度参数的统一接口
///   · 支持运行时切换难度
///   · 通过 EventBus 广播难度变更
///
/// 设计说明（GDD 4.3）：
///   · 三档预设：简单/中等/困难
///   · 各系统通过 DifficultySystem 获取修正值，而非硬编码
///   · 自定义难度通过修改预设的运行时副本实现
/// </summary>
public class DifficultySystem : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("难度预设")]
    [SerializeField] private DifficultyPresetSO _easyPreset;
    [SerializeField] private DifficultyPresetSO _normalPreset;
    [SerializeField] private DifficultyPresetSO _hardPreset;

    [Header("默认难度")]
    [SerializeField] private int _defaultPresetIndex = 1; // 0=Easy, 1=Normal, 2=Hard

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    private DifficultyPresetSO _currentPreset;

    // ══════════════════════════════════════════════════════
    // 属性 — 快捷访问
    // ══════════════════════════════════════════════════════

    public DifficultyPresetSO CurrentPreset => _currentPreset;
    public float DifficultyMultiplier => _currentPreset != null ? _currentPreset.DifficultyMultiplier : 1f;
    public float HungerDecayRate => _currentPreset != null ? _currentPreset.HungerDecayRate : 2f;
    public float ThirstDecayRate => _currentPreset != null ? _currentPreset.ThirstDecayRate : 3f;
    public float WarmthDecayRate => _currentPreset != null ? _currentPreset.WarmthDecayRate : 4f;
    public float StaminaRecoverRate => _currentPreset != null ? _currentPreset.StaminaRecoverRate : 5f;
    public float ShelterDecayModifier => _currentPreset != null ? _currentPreset.ShelterDecayModifier : 0.3f;
    public bool DeathPenaltyEnabled => _currentPreset != null && _currentPreset.DeathPenaltyEnabled;
    public bool OutdoorSaveEnabled => _currentPreset != null && _currentPreset.OutdoorSaveEnabled;
    public float EnemyDamageMultiplier => _currentPreset != null ? _currentPreset.EnemyDamageMultiplier : 1f;
    public float EnemyHealthMultiplier => _currentPreset != null ? _currentPreset.EnemyHealthMultiplier : 1f;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        ServiceLocator.Register<DifficultySystem>(this);

        // 设置默认难度
        switch (_defaultPresetIndex)
        {
            case 0: _currentPreset = _easyPreset; break;
            case 2: _currentPreset = _hardPreset; break;
            default: _currentPreset = _normalPreset; break;
        }
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<DifficultySystem>();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>切换到指定难度预设</summary>
    public void SetDifficulty(DifficultyPresetSO preset)
    {
        if (preset == null) return;

        var oldPreset = _currentPreset;
        _currentPreset = preset;

        EventBus.Publish(new DifficultyChangedEvent
        {
            OldPresetId = oldPreset != null ? oldPreset.PresetId : "",
            NewPresetId = preset.PresetId,
            NewDifficultyMultiplier = preset.DifficultyMultiplier
        });

        Debug.Log($"[Difficulty] 难度切换: {preset.DisplayName}");
    }

    /// <summary>切换到简单模式</summary>
    public void SetEasy()
    {
        if (_easyPreset != null) SetDifficulty(_easyPreset);
    }

    /// <summary>切换到中等模式</summary>
    public void SetNormal()
    {
        if (_normalPreset != null) SetDifficulty(_normalPreset);
    }

    /// <summary>切换到困难模式</summary>
    public void SetHard()
    {
        if (_hardPreset != null) SetDifficulty(_hardPreset);
    }

    /// <summary>获取难度预设数组（用于 UI 显示）</summary>
    public DifficultyPresetSO[] GetAllPresets()
    {
        return new[] { _easyPreset, _normalPreset, _hardPreset };
    }
}

/// <summary>难度变更事件</summary>
public struct DifficultyChangedEvent : IEvent
{
    public string OldPresetId;
    public string NewPresetId;
    public float NewDifficultyMultiplier;
}
