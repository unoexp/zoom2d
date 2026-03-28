// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Loading/LoadingPresenter.cs
// 加载界面Presenter。订阅加载事件，驱动ViewModel。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 加载界面 Presenter。
///
/// 核心职责：
///   · 订阅 LoadingStartedEvent / LoadingProgressEvent / LoadingCompletedEvent
///   · 开始加载时打开面板，完成后关闭
///   · 将事件数据写入 ViewModel
///
/// 设计说明：
///   · 订阅 GameStateChangedEvent 自动响应 Loading 状态
///   · 加载事件由场景管理器或 GameBootstrap 发布
/// </summary>
public class LoadingPresenter : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 引用
    // ══════════════════════════════════════════════════════

    [SerializeField] private LoadingPanelView _view;

    [Header("提示文本")]
    [SerializeField] private string[] _randomHints = new[]
    {
        "提示：庇护所内不会直接死亡",
        "提示：暴风雪时尽量回到庇护所",
        "提示：越深的地层材料越珍贵",
        "提示：工具升级可大幅提升挖掘效率",
        "提示：烹饪食物恢复量远高于生食"
    };

    private LoadingViewModel _viewModel;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        _viewModel = new LoadingViewModel();
    }

    private void Start()
    {
        if (_view != null)
        {
            _view.Bind(_viewModel);

            var uiManager = ServiceLocator.Get<UIManager>();
            if (uiManager != null)
            {
                uiManager.RegisterPanel(_view);
            }
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<LoadingStartedEvent>(OnLoadingStarted);
        EventBus.Subscribe<LoadingProgressEvent>(OnLoadingProgress);
        EventBus.Subscribe<LoadingCompletedEvent>(OnLoadingCompleted);
        EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<LoadingStartedEvent>(OnLoadingStarted);
        EventBus.Unsubscribe<LoadingProgressEvent>(OnLoadingProgress);
        EventBus.Unsubscribe<LoadingCompletedEvent>(OnLoadingCompleted);
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    private void OnDestroy()
    {
        if (_view != null)
        {
            var uiManager = ServiceLocator.Get<UIManager>();
            if (uiManager != null)
            {
                uiManager.UnregisterPanel(_view);
            }
        }
    }

    // ══════════════════════════════════════════════════════
    // 事件处理
    // ══════════════════════════════════════════════════════

    private void OnLoadingStarted(LoadingStartedEvent evt)
    {
        _viewModel.Reset();

        // 设置提示文本（优先使用事件携带的，否则随机选择）
        string hint = !string.IsNullOrEmpty(evt.HintText)
            ? evt.HintText
            : GetRandomHint();
        _viewModel.SetHint(hint);
        _viewModel.SetProgress(0f, "准备中...");

        ShowPanel();
    }

    private void OnLoadingProgress(LoadingProgressEvent evt)
    {
        _viewModel.SetProgress(evt.Progress, evt.StepDescription);
    }

    private void OnLoadingCompleted(LoadingCompletedEvent evt)
    {
        _viewModel.SetProgress(1f, "加载完成");
        HidePanel();
    }

    private void OnGameStateChanged(GameStateChangedEvent evt)
    {
        // 进入 Loading 状态时自动显示
        if (evt.NewState == GameState.Loading)
        {
            _viewModel.Reset();
            _viewModel.SetHint(GetRandomHint());
            _viewModel.SetProgress(0f, "加载中...");
            ShowPanel();
        }
        // 离开 Loading 状态时自动隐藏
        else if (evt.PreviousState == GameState.Loading)
        {
            HidePanel();
        }
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    private void ShowPanel()
    {
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null && _view != null)
        {
            uiManager.OpenPanel(_view);
        }
    }

    private void HidePanel()
    {
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null && _view != null)
        {
            uiManager.ClosePanel(_view);
        }
    }

    private string GetRandomHint()
    {
        if (_randomHints == null || _randomHints.Length == 0)
            return string.Empty;
        return _randomHints[Random.Range(0, _randomHints.Length)];
    }
}
