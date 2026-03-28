// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Dialog/Presenters/DialogPresenter.cs
// 对话Presenter。驱动对话流程，连接数据与View。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 对话 Presenter。
///
/// 核心职责：
///   · 监听 DialogStartedEvent，加载对话数据并显示
///   · 驱动对话节点推进（继续/选择分支）
///   · 处理对话选择的信任度变化
///   · 对话结束时发布 DialogEndedEvent
/// </summary>
public class DialogPresenter : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 引用
    // ══════════════════════════════════════════════════════

    [SerializeField] private DialogPanelView _panelView;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    private DialogDataSO _currentDialog;
    private int _currentNodeIndex;
    private string _currentNPCId;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Start()
    {
        if (_panelView != null)
        {
            _panelView.OnContinueClicked += HandleContinue;
            _panelView.OnChoiceSelected += HandleChoice;

            var uiManager = ServiceLocator.Get<UIManager>();
            if (uiManager != null)
                uiManager.RegisterPanel(_panelView);
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<DialogStartedEvent>(OnDialogStarted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DialogStartedEvent>(OnDialogStarted);
    }

    private void OnDestroy()
    {
        if (_panelView != null)
        {
            _panelView.OnContinueClicked -= HandleContinue;
            _panelView.OnChoiceSelected -= HandleChoice;
        }
    }

    // ══════════════════════════════════════════════════════
    // 事件处理
    // ══════════════════════════════════════════════════════

    private void OnDialogStarted(DialogStartedEvent evt)
    {
        _currentNPCId = evt.NPCId;

        // 查找对话数据（通过 NPC 定义获取）
        _currentDialog = FindDialogData(evt.DialogId);
        if (_currentDialog == null || _currentDialog.Nodes == null || _currentDialog.Nodes.Length == 0)
        {
            Debug.LogWarning($"[DialogPresenter] 未找到对话数据: {evt.DialogId}");
            return;
        }

        _currentNodeIndex = 0;
        ShowCurrentNode();

        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null)
            uiManager.OpenPanel(_panelView);
    }

    // ══════════════════════════════════════════════════════
    // 对话流程
    // ══════════════════════════════════════════════════════

    /// <summary>显示当前节点</summary>
    private void ShowCurrentNode()
    {
        if (_currentDialog == null) return;
        if (_currentNodeIndex < 0 || _currentNodeIndex >= _currentDialog.Nodes.Length)
        {
            EndDialog();
            return;
        }

        var node = _currentDialog.Nodes[_currentNodeIndex];
        bool hasChoices = node.Choices != null && node.Choices.Length > 0;

        _panelView.ShowNode(node.SpeakerName, node.SpeakerPortrait, node.Content, hasChoices);

        if (hasChoices)
        {
            _panelView.ShowChoices(node.Choices);
        }
        else
        {
            _panelView.ClearChoices();
        }

        // 广播节点推进
        EventBus.Publish(new DialogNodeAdvancedEvent
        {
            SpeakerName = node.SpeakerName,
            Content = node.Content,
            NodeIndex = _currentNodeIndex,
            HasChoices = hasChoices
        });

        // 触发事件
        if (!string.IsNullOrEmpty(node.TriggerEventId))
        {
            Debug.Log($"[Dialog] 触发事件: {node.TriggerEventId}");
        }
    }

    /// <summary>继续下一节点</summary>
    private void HandleContinue()
    {
        if (_currentDialog == null) return;

        var node = _currentDialog.Nodes[_currentNodeIndex];
        _currentNodeIndex = node.NextNodeIndex;

        if (_currentNodeIndex < 0)
        {
            EndDialog();
        }
        else
        {
            ShowCurrentNode();
        }
    }

    /// <summary>选择分支</summary>
    private void HandleChoice(int choiceIndex)
    {
        if (_currentDialog == null) return;

        var node = _currentDialog.Nodes[_currentNodeIndex];
        if (node.Choices == null || choiceIndex < 0 || choiceIndex >= node.Choices.Length) return;

        var choice = node.Choices[choiceIndex];

        // 应用信任度变化
        if (choice.TrustDelta != 0)
        {
            ApplyTrustChange(choice.TrustDelta);
        }

        _currentNodeIndex = choice.TargetNodeIndex;

        if (_currentNodeIndex < 0)
        {
            EndDialog();
        }
        else
        {
            ShowCurrentNode();
        }
    }

    /// <summary>结束对话</summary>
    private void EndDialog()
    {
        _panelView.ClearChoices();

        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null)
            uiManager.ClosePanel(_panelView);

        EventBus.Publish(new DialogEndedEvent
        {
            DialogId = _currentDialog != null ? _currentDialog.DialogId : "",
            NPCId = _currentNPCId
        });

        _currentDialog = null;
        _currentNPCId = null;
    }

    // ══════════════════════════════════════════════════════
    // 辅助
    // ══════════════════════════════════════════════════════

    /// <summary>查找对话数据（简单实现，后续可扩展为注册表）</summary>
    private DialogDataSO FindDialogData(string dialogId)
    {
        // 通过 Resources 加载，路径约定: Resources/Dialogs/{dialogId}
        var dialog = Resources.Load<DialogDataSO>($"Dialogs/{dialogId}");
        if (dialog != null) return dialog;

        // 回退：在已加载的 ScriptableObject 中查找
        var all = Resources.FindObjectsOfTypeAll<DialogDataSO>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].DialogId == dialogId) return all[i];
        }

        return null;
    }

    /// <summary>应用信任度变化</summary>
    private void ApplyTrustChange(int delta)
    {
        if (string.IsNullOrEmpty(_currentNPCId)) return;

        // 查找场景中对应的 NPCController
        var npcs = FindObjectsOfType<NPCController>();
        for (int i = 0; i < npcs.Length; i++)
        {
            if (npcs[i].Definition != null && npcs[i].Definition.NPCId == _currentNPCId)
            {
                npcs[i].ModifyTrust(delta);
                break;
            }
        }
    }
}
