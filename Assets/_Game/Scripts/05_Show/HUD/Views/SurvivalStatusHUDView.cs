// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/HUD/Views/SurvivalStatusHUDView.cs
// 生存状态HUD面板View。包含所有状态条的容器。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;
using TMPro;

/// <summary>
/// 生存状态HUD面板View。
///
/// 职责：
///   · 持有所有 SurvivalStatusBarView 引用
///   · 监听 ViewModel 事件，分发给对应的状态条
///   · 管理消耗品使用反馈文本的显示
/// </summary>
public class SurvivalStatusHUDView : UIPanel
{
    // ══════════════════════════════════════════════════════
    // UI引用
    // ══════════════════════════════════════════════════════

    [Header("状态条")]
    [SerializeField] private SurvivalStatusBarView[] _statusBars;

    [Header("反馈提示")]
    [SerializeField] private TextMeshProUGUI _feedbackText;
    [SerializeField] private float _feedbackDuration = 2f;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    private SurvivalHUDViewModel _viewModel;
    private float _feedbackTimer;

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>绑定 ViewModel</summary>
    public void Bind(SurvivalHUDViewModel viewModel)
    {
        // 解绑旧的
        if (_viewModel != null)
        {
            _viewModel.OnAttributeUpdated -= HandleAttributeUpdated;
            _viewModel.OnWarningChanged -= HandleWarningChanged;
            _viewModel.OnConsumableFeedback -= HandleConsumableFeedback;
        }

        _viewModel = viewModel;

        if (_viewModel != null)
        {
            _viewModel.OnAttributeUpdated += HandleAttributeUpdated;
            _viewModel.OnWarningChanged += HandleWarningChanged;
            _viewModel.OnConsumableFeedback += HandleConsumableFeedback;
        }
    }

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    protected override void Awake()
    {
        base.Awake();

        // 自动收集子物体中的状态条（如果未手动赋值）
        if (_statusBars == null || _statusBars.Length == 0)
        {
            _statusBars = GetComponentsInChildren<SurvivalStatusBarView>(true);
        }

        // 隐藏反馈文本
        if (_feedbackText != null)
        {
            _feedbackText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // 反馈文本定时隐藏
        if (_feedbackTimer > 0f)
        {
            _feedbackTimer -= Time.unscaledDeltaTime;
            if (_feedbackTimer <= 0f && _feedbackText != null)
            {
                _feedbackText.gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        Bind(null); // 清理订阅
    }

    // ══════════════════════════════════════════════════════
    // ViewModel 事件处理
    // ══════════════════════════════════════════════════════

    private void HandleAttributeUpdated(AttributeDisplayData data)
    {
        if (_statusBars == null) return;

        // [PERF] 线性查找匹配的状态条
        for (int i = 0; i < _statusBars.Length; i++)
        {
            if (_statusBars[i] != null && _statusBars[i].AttributeType == data.Type)
            {
                _statusBars[i].UpdateDisplay(data);
                break;
            }
        }
    }

    private void HandleWarningChanged(SurvivalAttributeType type, CriticalWarningLevel level)
    {
        if (_statusBars == null) return;

        for (int i = 0; i < _statusBars.Length; i++)
        {
            if (_statusBars[i] != null && _statusBars[i].AttributeType == type)
            {
                _statusBars[i].SetWarningLevel((int)level);
                break;
            }
        }
    }

    private void HandleConsumableFeedback(string displayName)
    {
        if (_feedbackText == null) return;

        _feedbackText.text = $"使用了 {displayName}";
        _feedbackText.gameObject.SetActive(true);
        _feedbackTimer = _feedbackDuration;
    }
}
