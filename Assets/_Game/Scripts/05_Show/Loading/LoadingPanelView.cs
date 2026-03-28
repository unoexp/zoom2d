// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Loading/LoadingPanelView.cs
// 加载界面View。显示进度条、提示文本。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 加载界面 View。
///
/// 核心职责：
///   · 显示加载进度条
///   · 显示提示文本和当前步骤描述
///   · 绑定 ViewModel 事件驱动更新
///
/// 设计说明：
///   · 继承 UIPanel，全屏覆盖，由 UIManager 管理
///   · 打开时暂停游戏时间
/// </summary>
public class LoadingPanelView : UIPanel
{
    // ══════════════════════════════════════════════════════
    // UI 引用
    // ══════════════════════════════════════════════════════

    [Header("进度条")]
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _progressText;

    [Header("文本")]
    [SerializeField] private TextMeshProUGUI _hintText;
    [SerializeField] private TextMeshProUGUI _stepText;

    // ══════════════════════════════════════════════════════
    // ViewModel
    // ══════════════════════════════════════════════════════

    private LoadingViewModel _viewModel;

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>绑定 ViewModel</summary>
    public void Bind(LoadingViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnProgressChanged -= OnProgressChanged;
            _viewModel.OnHintChanged -= OnHintChanged;
            _viewModel.OnStepChanged -= OnStepChanged;
        }

        _viewModel = viewModel;

        if (_viewModel != null)
        {
            _viewModel.OnProgressChanged += OnProgressChanged;
            _viewModel.OnHintChanged += OnHintChanged;
            _viewModel.OnStepChanged += OnStepChanged;
        }
    }

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void OnDestroy()
    {
        Bind(null);
    }

    // ══════════════════════════════════════════════════════
    // ViewModel 事件回调
    // ══════════════════════════════════════════════════════

    private void OnProgressChanged(float progress)
    {
        if (_progressBar != null)
            _progressBar.value = progress;

        if (_progressText != null)
            _progressText.text = $"{(int)(progress * 100)}%";
    }

    private void OnHintChanged(string hint)
    {
        if (_hintText != null)
            _hintText.text = hint;
    }

    private void OnStepChanged(string step)
    {
        if (_stepText != null)
            _stepText.text = step;
    }
}
