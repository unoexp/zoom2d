// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Quest/Presenters/QuestLogPresenter.cs
// 任务日志Presenter。连接QuestSystem与任务日志UI。
// ══════════════════════════════════════════════════════════════════════
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 任务日志 Presenter。
///
/// 核心职责：
///   · 订阅任务事件（激活/进度/完成）并更新 ViewModel
///   · 从 QuestSystem 拉取任务数据构建显示列表
///   · 处理 View 层的交互事件（选中、标签切换）
/// </summary>
public class QuestLogPresenter : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 引用
    // ══════════════════════════════════════════════════════

    [SerializeField] private QuestLogPanelView _view;

    private QuestLogViewModel _viewModel;
    private QuestSystem _questSystem;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        _viewModel = new QuestLogViewModel();
    }

    private void Start()
    {
        _questSystem = ServiceLocator.Get<QuestSystem>();

        if (_view != null)
        {
            _view.Bind(_viewModel);

            var uiManager = ServiceLocator.Get<UIManager>();
            if (uiManager != null)
            {
                uiManager.RegisterPanel(_view);
            }

            // 绑定 View 交互事件
            _view.OnQuestItemClicked += OnQuestItemClicked;
            _view.OnTabSwitched += OnTabSwitched;
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<QuestActivatedEvent>(OnQuestActivated);
        EventBus.Subscribe<QuestObjectiveProgressEvent>(OnObjectiveProgress);
        EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<QuestActivatedEvent>(OnQuestActivated);
        EventBus.Unsubscribe<QuestObjectiveProgressEvent>(OnObjectiveProgress);
        EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
    }

    private void OnDestroy()
    {
        if (_view != null)
        {
            _view.OnQuestItemClicked -= OnQuestItemClicked;
            _view.OnTabSwitched -= OnTabSwitched;

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

    private void OnQuestActivated(QuestActivatedEvent evt)
    {
        RefreshQuestLists();
    }

    private void OnObjectiveProgress(QuestObjectiveProgressEvent evt)
    {
        _viewModel.UpdateObjectiveProgress(
            evt.QuestId, evt.ObjectiveIndex,
            evt.CurrentAmount, evt.IsCompleted);
    }

    private void OnQuestCompleted(QuestCompletedEvent evt)
    {
        RefreshQuestLists();
    }

    // ══════════════════════════════════════════════════════
    // View 交互处理
    // ══════════════════════════════════════════════════════

    private void OnQuestItemClicked(int index)
    {
        _viewModel.SelectQuest(index);
    }

    private void OnTabSwitched(bool showCompleted)
    {
        _viewModel.ToggleTab(showCompleted);
    }

    // ══════════════════════════════════════════════════════
    // 数据构建
    // ══════════════════════════════════════════════════════

    /// <summary>从 QuestSystem 拉取数据刷新 ViewModel</summary>
    private void RefreshQuestLists()
    {
        if (_questSystem == null) return;

        var activeDefs = _questSystem.GetActiveQuests();
        var completedDefs = _questSystem.GetCompletedQuests();

        var activeDisplay = new List<QuestDisplayData>(activeDefs.Count);
        for (int i = 0; i < activeDefs.Count; i++)
        {
            activeDisplay.Add(BuildDisplayData(activeDefs[i], QuestState.Active));
        }

        var completedDisplay = new List<QuestDisplayData>(completedDefs.Count);
        for (int i = 0; i < completedDefs.Count; i++)
        {
            completedDisplay.Add(BuildDisplayData(completedDefs[i], QuestState.Completed));
        }

        _viewModel.RefreshLists(activeDisplay, completedDisplay);
    }

    /// <summary>将 QuestDefinitionSO + 运行时数据转换为显示数据</summary>
    private QuestDisplayData BuildDisplayData(QuestDefinitionSO def, QuestState state)
    {
        var runtime = _questSystem.GetRuntimeData(def.QuestId);

        QuestObjectiveDisplayData[] objectives = null;
        if (def.Objectives != null && def.Objectives.Length > 0)
        {
            objectives = new QuestObjectiveDisplayData[def.Objectives.Length];
            for (int i = 0; i < def.Objectives.Length; i++)
            {
                var obj = def.Objectives[i];
                objectives[i] = new QuestObjectiveDisplayData
                {
                    Description = obj.Description,
                    CurrentAmount = runtime != null && runtime.ObjectiveProgress != null && i < runtime.ObjectiveProgress.Length
                        ? runtime.ObjectiveProgress[i] : 0,
                    RequiredAmount = obj.RequiredAmount,
                    IsCompleted = runtime != null && runtime.ObjectiveCompleted != null && i < runtime.ObjectiveCompleted.Length
                        && runtime.ObjectiveCompleted[i],
                    IsOptional = obj.IsOptional
                };
            }
        }

        return new QuestDisplayData
        {
            QuestId = def.QuestId,
            DisplayName = def.DisplayName,
            Description = def.Description,
            IsMainQuest = def.IsMainQuest,
            State = state,
            Objectives = objectives
        };
    }

    // ══════════════════════════════════════════════════════
    // 公有 API（供外部打开面板时刷新）
    // ══════════════════════════════════════════════════════

    /// <summary>打开任务日志面板</summary>
    public void OpenQuestLog()
    {
        RefreshQuestLists();
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null && _view != null)
        {
            uiManager.OpenPanel(_view);
        }
    }
}
