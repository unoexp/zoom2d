// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Quest/ViewModels/QuestLogViewModel.cs
// 任务日志ViewModel。管理任务列表和选中任务的显示数据。
// ══════════════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;

/// <summary>
/// 单条任务的 UI 显示数据
/// </summary>
public struct QuestDisplayData
{
    public string QuestId;
    public string DisplayName;
    public string Description;
    public bool IsMainQuest;
    public QuestState State;
    public QuestObjectiveDisplayData[] Objectives;
}

/// <summary>
/// 单个任务目标的 UI 显示数据
/// </summary>
public struct QuestObjectiveDisplayData
{
    public string Description;
    public int CurrentAmount;
    public int RequiredAmount;
    public bool IsCompleted;
    public bool IsOptional;
}

/// <summary>
/// 任务日志 ViewModel。
///
/// 核心职责：
///   · 持有活跃任务列表和已完成任务列表
///   · 管理当前选中任务的详情数据
///   · 暴露事件通知 View 更新
/// </summary>
public class QuestLogViewModel
{
    // ══════════════════════════════════════════════════════
    // 数据
    // ══════════════════════════════════════════════════════

    private readonly List<QuestDisplayData> _activeQuests = new List<QuestDisplayData>();
    private readonly List<QuestDisplayData> _completedQuests = new List<QuestDisplayData>();
    private int _selectedIndex = -1;
    private bool _showingCompleted;

    // ══════════════════════════════════════════════════════
    // 事件
    // ══════════════════════════════════════════════════════

    /// <summary>任务列表刷新</summary>
    public event Action OnQuestListUpdated;

    /// <summary>选中任务变化</summary>
    public event Action<QuestDisplayData> OnSelectedQuestChanged;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    public IReadOnlyList<QuestDisplayData> ActiveQuests => _activeQuests;
    public IReadOnlyList<QuestDisplayData> CompletedQuests => _completedQuests;
    public int SelectedIndex => _selectedIndex;
    public bool ShowingCompleted => _showingCompleted;
    public int ActiveCount => _activeQuests.Count;
    public int CompletedCount => _completedQuests.Count;

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>刷新任务列表</summary>
    public void RefreshLists(List<QuestDisplayData> active, List<QuestDisplayData> completed)
    {
        _activeQuests.Clear();
        _completedQuests.Clear();

        if (active != null)
        {
            for (int i = 0; i < active.Count; i++)
                _activeQuests.Add(active[i]);
        }

        if (completed != null)
        {
            for (int i = 0; i < completed.Count; i++)
                _completedQuests.Add(completed[i]);
        }

        OnQuestListUpdated?.Invoke();

        // 自动选中第一条
        if (CurrentList.Count > 0)
            SelectQuest(0);
        else
            ClearSelection();
    }

    /// <summary>选中指定索引的任务</summary>
    public void SelectQuest(int index)
    {
        var list = CurrentList;
        if (index < 0 || index >= list.Count) return;

        _selectedIndex = index;
        OnSelectedQuestChanged?.Invoke(list[index]);
    }

    /// <summary>切换显示活跃/已完成列表</summary>
    public void ToggleTab(bool showCompleted)
    {
        _showingCompleted = showCompleted;
        _selectedIndex = -1;
        OnQuestListUpdated?.Invoke();

        if (CurrentList.Count > 0)
            SelectQuest(0);
        else
            ClearSelection();
    }

    /// <summary>更新单条任务的目标进度</summary>
    public void UpdateObjectiveProgress(string questId, int objectiveIndex,
                                         int currentAmount, bool isCompleted)
    {
        for (int i = 0; i < _activeQuests.Count; i++)
        {
            if (_activeQuests[i].QuestId != questId) continue;

            var quest = _activeQuests[i];
            if (quest.Objectives != null
                && objectiveIndex >= 0
                && objectiveIndex < quest.Objectives.Length)
            {
                var obj = quest.Objectives[objectiveIndex];
                obj.CurrentAmount = currentAmount;
                obj.IsCompleted = isCompleted;
                quest.Objectives[objectiveIndex] = obj;
                _activeQuests[i] = quest;
            }

            // 如果是当前选中的任务，刷新详情
            if (_selectedIndex == i && !_showingCompleted)
            {
                OnSelectedQuestChanged?.Invoke(_activeQuests[i]);
            }
            return;
        }
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    private IReadOnlyList<QuestDisplayData> CurrentList =>
        _showingCompleted ? (IReadOnlyList<QuestDisplayData>)_completedQuests : _activeQuests;

    private void ClearSelection()
    {
        _selectedIndex = -1;
        OnSelectedQuestChanged?.Invoke(default);
    }
}
