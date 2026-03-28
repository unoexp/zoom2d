// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/HUD/Presenters/SurvivalHUDPresenter.cs
// 生存状态HUD的Presenter。连接业务事件与ViewModel。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 生存状态HUD Presenter。
///
/// 核心职责：
///   · 订阅 EventBus 的生存属性变化事件
///   · 将业务数据写入 ViewModel（ViewModel 通知 View 更新）
///   · 不直接操作 View 组件
///
/// 设计说明：
///   · 遵循 Presenter → ViewModel → View 单向数据流
///   · 初始化时从 SurvivalStatusSystem 拉取当前值
///   · 运行时通过事件驱动更新
/// </summary>
public class SurvivalHUDPresenter : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 引用
    // ══════════════════════════════════════════════════════

    [SerializeField] private SurvivalStatusHUDView _hudView;

    private SurvivalHUDViewModel _viewModel;
    private SurvivalStatusSystem _survivalSystem;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        _viewModel = new SurvivalHUDViewModel();
    }

    private void Start()
    {
        _survivalSystem = ServiceLocator.Get<SurvivalStatusSystem>();

        // 绑定 View 和 ViewModel
        if (_hudView != null)
        {
            _hudView.Bind(_viewModel);
        }

        // 注册为 HUD（常驻显示，不入栈）
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null && _hudView != null)
        {
            uiManager.RegisterPanel(_hudView);
            uiManager.RegisterHUD(_hudView);
            _hudView.Show();
        }

        // 初始化拉取所有属性的当前值
        InitializeAttributeValues();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<SurvivalAttributeChangedEvent>(OnAttributeChanged);
        EventBus.Subscribe<SurvivalCriticalWarningEvent>(OnCriticalWarning);
        EventBus.Subscribe<ConsumableUsedEvent>(OnConsumableUsed);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SurvivalAttributeChangedEvent>(OnAttributeChanged);
        EventBus.Unsubscribe<SurvivalCriticalWarningEvent>(OnCriticalWarning);
        EventBus.Unsubscribe<ConsumableUsedEvent>(OnConsumableUsed);
    }

    // ══════════════════════════════════════════════════════
    // 初始化
    // ══════════════════════════════════════════════════════

    /// <summary>从 SurvivalStatusSystem 拉取当前值初始化 ViewModel</summary>
    private void InitializeAttributeValues()
    {
        if (_survivalSystem == null) return;

        var trackedTypes = SurvivalHUDViewModel.TrackedTypes;
        for (int i = 0; i < trackedTypes.Length; i++)
        {
            var type = trackedTypes[i];
            float current = _survivalSystem.GetValue(type);
            float max = _survivalSystem.GetMaxValue(type);
            _viewModel.UpdateAttribute(type, current, max);
        }
    }

    // ══════════════════════════════════════════════════════
    // 事件处理
    // ══════════════════════════════════════════════════════

    /// <summary>生存属性变化 → 更新 ViewModel</summary>
    private void OnAttributeChanged(SurvivalAttributeChangedEvent evt)
    {
        _viewModel.UpdateAttribute(evt.AttributeType, evt.NewValue, evt.MaxValue);
    }

    /// <summary>临界预警 → 更新 ViewModel 预警状态</summary>
    private void OnCriticalWarning(SurvivalCriticalWarningEvent evt)
    {
        _viewModel.UpdateWarning(evt.AttributeType, evt.WarningLevel);
    }

    /// <summary>消耗品使用 → 显示反馈</summary>
    private void OnConsumableUsed(ConsumableUsedEvent evt)
    {
        _viewModel.ShowConsumableFeedback(evt.DisplayName);
    }
}
