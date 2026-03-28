// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Loading/LoadingViewModel.cs
// 加载界面ViewModel。管理加载进度和提示文本。
// ══════════════════════════════════════════════════════════════════════
using System;

/// <summary>
/// 加载界面 ViewModel。
///
/// 核心职责：
///   · 持有加载进度和提示文本
///   · 暴露事件通知 View 更新
/// </summary>
public class LoadingViewModel
{
    // ══════════════════════════════════════════════════════
    // 数据
    // ══════════════════════════════════════════════════════

    private float _progress;
    private string _hintText = string.Empty;
    private string _stepDescription = string.Empty;

    // ══════════════════════════════════════════════════════
    // 事件
    // ══════════════════════════════════════════════════════

    /// <summary>进度更新</summary>
    public event Action<float> OnProgressChanged;

    /// <summary>提示文本更新</summary>
    public event Action<string> OnHintChanged;

    /// <summary>步骤描述更新</summary>
    public event Action<string> OnStepChanged;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    public float Progress => _progress;
    public string HintText => _hintText;
    public string StepDescription => _stepDescription;

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>设置加载提示文本</summary>
    public void SetHint(string hint)
    {
        _hintText = hint ?? string.Empty;
        OnHintChanged?.Invoke(_hintText);
    }

    /// <summary>更新进度（0-1）</summary>
    public void SetProgress(float progress, string stepDescription = null)
    {
        _progress = UnityEngine.Mathf.Clamp01(progress);
        OnProgressChanged?.Invoke(_progress);

        if (stepDescription != null)
        {
            _stepDescription = stepDescription;
            OnStepChanged?.Invoke(_stepDescription);
        }
    }

    /// <summary>重置状态</summary>
    public void Reset()
    {
        _progress = 0f;
        _hintText = string.Empty;
        _stepDescription = string.Empty;
    }
}
