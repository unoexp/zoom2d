// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Quest/Views/QuestLogPanelView.cs
// 任务日志面板View。显示任务列表和任务详情。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 任务日志面板 View。
///
/// 核心职责：
///   · 显示活跃/已完成任务列表
///   · 显示选中任务的详情和目标进度
///   · 提供标签切换（活跃/已完成）
///   · 绑定 ViewModel 事件驱动更新
/// </summary>
public class QuestLogPanelView : UIPanel
{
    // ══════════════════════════════════════════════════════
    // UI 引用
    // ══════════════════════════════════════════════════════

    [Header("标签按钮")]
    [SerializeField] private Button _activeTabButton;
    [SerializeField] private Button _completedTabButton;
    [SerializeField] private TextMeshProUGUI _activeTabText;
    [SerializeField] private TextMeshProUGUI _completedTabText;

    [Header("任务列表")]
    [SerializeField] private Transform _questListContainer;
    [SerializeField] private GameObject _questItemTemplate;

    [Header("任务详情")]
    [SerializeField] private TextMeshProUGUI _questNameText;
    [SerializeField] private TextMeshProUGUI _questDescriptionText;
    [SerializeField] private Transform _objectivesContainer;
    [SerializeField] private GameObject _objectiveTemplate;

    [Header("空状态")]
    [SerializeField] private GameObject _emptyStateGroup;
    [SerializeField] private TextMeshProUGUI _emptyStateText;

    // ══════════════════════════════════════════════════════
    // ViewModel
    // ══════════════════════════════════════════════════════

    private QuestLogViewModel _viewModel;

    /// <summary>任务条目点击事件</summary>
    public event System.Action<int> OnQuestItemClicked;

    /// <summary>标签切换事件</summary>
    public event System.Action<bool> OnTabSwitched;

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>绑定 ViewModel</summary>
    public void Bind(QuestLogViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnQuestListUpdated -= RefreshList;
            _viewModel.OnSelectedQuestChanged -= RefreshDetail;
        }

        _viewModel = viewModel;

        if (_viewModel != null)
        {
            _viewModel.OnQuestListUpdated += RefreshList;
            _viewModel.OnSelectedQuestChanged += RefreshDetail;
        }
    }

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    protected override void Awake()
    {
        base.Awake();

        if (_questItemTemplate != null)
            _questItemTemplate.SetActive(false);
        if (_objectiveTemplate != null)
            _objectiveTemplate.SetActive(false);

        if (_activeTabButton != null)
            _activeTabButton.onClick.AddListener(() => OnTabSwitched?.Invoke(false));
        if (_completedTabButton != null)
            _completedTabButton.onClick.AddListener(() => OnTabSwitched?.Invoke(true));
    }

    private void OnDestroy()
    {
        Bind(null);
        if (_activeTabButton != null) _activeTabButton.onClick.RemoveAllListeners();
        if (_completedTabButton != null) _completedTabButton.onClick.RemoveAllListeners();
    }

    // ══════════════════════════════════════════════════════
    // 刷新方法
    // ══════════════════════════════════════════════════════

    private void RefreshList()
    {
        if (_viewModel == null || _questListContainer == null) return;

        // 清理旧列表
        ClearContainer(_questListContainer);

        var list = _viewModel.ShowingCompleted
            ? _viewModel.CompletedQuests
            : _viewModel.ActiveQuests;

        // 更新标签文本
        if (_activeTabText != null)
            _activeTabText.text = $"进行中 ({_viewModel.ActiveCount})";
        if (_completedTabText != null)
            _completedTabText.text = $"已完成 ({_viewModel.CompletedCount})";

        // 空状态
        if (_emptyStateGroup != null)
        {
            bool isEmpty = list.Count == 0;
            _emptyStateGroup.SetActive(isEmpty);
            if (isEmpty && _emptyStateText != null)
            {
                _emptyStateText.text = _viewModel.ShowingCompleted
                    ? "尚未完成任何任务" : "暂无进行中的任务";
            }
        }

        // 生成列表条目
        if (_questItemTemplate == null) return;
        for (int i = 0; i < list.Count; i++)
        {
            var quest = list[i];
            var go = Instantiate(_questItemTemplate, _questListContainer);
            go.SetActive(true);

            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                string prefix = quest.IsMainQuest ? "[主线] " : "";
                text.text = $"{prefix}{quest.DisplayName}";
            }

            // 点击选中
            int index = i;
            var button = go.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnQuestItemClicked?.Invoke(index));
            }
        }
    }

    private void RefreshDetail(QuestDisplayData data)
    {
        if (string.IsNullOrEmpty(data.QuestId))
        {
            if (_questNameText != null) _questNameText.text = string.Empty;
            if (_questDescriptionText != null) _questDescriptionText.text = string.Empty;
            if (_objectivesContainer != null) ClearContainer(_objectivesContainer);
            return;
        }

        if (_questNameText != null)
            _questNameText.text = data.DisplayName;

        if (_questDescriptionText != null)
            _questDescriptionText.text = data.Description;

        // 目标列表
        if (_objectivesContainer != null && _objectiveTemplate != null)
        {
            ClearContainer(_objectivesContainer);

            if (data.Objectives != null)
            {
                for (int i = 0; i < data.Objectives.Length; i++)
                {
                    var obj = data.Objectives[i];
                    var go = Instantiate(_objectiveTemplate, _objectivesContainer);
                    go.SetActive(true);

                    var text = go.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        string checkmark = obj.IsCompleted ? "✓ " : "○ ";
                        string optional = obj.IsOptional ? " (可选)" : "";
                        string progress = obj.RequiredAmount > 1
                            ? $" [{obj.CurrentAmount}/{obj.RequiredAmount}]" : "";
                        text.text = $"{checkmark}{obj.Description}{progress}{optional}";
                        text.color = obj.IsCompleted
                            ? new Color(0.5f, 0.9f, 0.5f)
                            : Color.white;
                    }
                }
            }
        }
    }

    /// <summary>清理容器下所有激活的子对象</summary>
    private static void ClearContainer(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            var child = container.GetChild(i);
            if (child.gameObject.activeSelf)
                Destroy(child.gameObject);
        }
    }
}
