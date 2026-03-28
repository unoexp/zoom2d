// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Dialog/Views/DialogPanelView.cs
// 对话面板View。显示 NPC 对话内容和玩家选择项。
// ══════════════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 对话面板View。
///
/// 职责：
///   · 显示说话者名称、头像、对话内容
///   · 渲染玩家选择项按钮
///   · 点击继续/选择时通知 Presenter
/// </summary>
public class DialogPanelView : UIPanel
{
    // ══════════════════════════════════════════════════════
    // UI引用
    // ══════════════════════════════════════════════════════

    [Header("对话内容")]
    [SerializeField] private TextMeshProUGUI _speakerNameText;
    [SerializeField] private Image _speakerPortrait;
    [SerializeField] private TextMeshProUGUI _contentText;

    [Header("选择项")]
    [SerializeField] private Transform _choiceContainer;
    [SerializeField] private GameObject _choicePrefab;

    [Header("继续按钮")]
    [SerializeField] private Button _continueButton;

    // ══════════════════════════════════════════════════════
    // 事件
    // ══════════════════════════════════════════════════════

    /// <summary>点击继续（无选择项时）</summary>
    public event Action OnContinueClicked;

    /// <summary>选择某个选项</summary>
    public event Action<int> OnChoiceSelected;

    // ══════════════════════════════════════════════════════
    // 运行时
    // ══════════════════════════════════════════════════════

    private readonly List<GameObject> _choiceInstances = new List<GameObject>();

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    protected override void Awake()
    {
        base.Awake();
        if (_continueButton != null)
            _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
    }

    private void OnDestroy()
    {
        ClearChoices();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>显示对话节点</summary>
    public void ShowNode(string speakerName, Sprite portrait, string content, bool hasChoices)
    {
        if (_speakerNameText != null)
            _speakerNameText.text = string.IsNullOrEmpty(speakerName) ? "" : speakerName;

        if (_speakerPortrait != null)
        {
            _speakerPortrait.sprite = portrait;
            _speakerPortrait.gameObject.SetActive(portrait != null);
        }

        if (_contentText != null)
            _contentText.text = content;

        // 有选择项时隐藏继续按钮，否则显示
        if (_continueButton != null)
            _continueButton.gameObject.SetActive(!hasChoices);
    }

    /// <summary>显示选择项</summary>
    public void ShowChoices(DialogChoice[] choices)
    {
        ClearChoices();

        if (_choiceContainer == null || _choicePrefab == null || choices == null) return;

        for (int i = 0; i < choices.Length; i++)
        {
            var instance = Instantiate(_choicePrefab, _choiceContainer);
            _choiceInstances.Add(instance);

            var text = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = choices[i].Text;

            int index = i;
            var button = instance.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => OnChoiceSelected?.Invoke(index));
        }
    }

    /// <summary>清除选择项</summary>
    public void ClearChoices()
    {
        for (int i = 0; i < _choiceInstances.Count; i++)
        {
            if (_choiceInstances[i] != null)
                Destroy(_choiceInstances[i]);
        }
        _choiceInstances.Clear();
    }
}
