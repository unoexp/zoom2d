// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/MainMenu/MainMenuView.cs
// 主菜单面板View。继承UIPanel，处理主菜单按钮交互。
// ══════════════════════════════════════════════════════════════════════
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 主菜单面板View。
///
/// 职责：
///   · 显示主菜单UI（新游戏、继续、设置、退出）
///   · 接收按钮点击事件，通过委托通知 Presenter
///   · 管理按钮可用状态（如无存档时「继续」不可点）
/// </summary>
public class MainMenuView : UIPanel
{
    // ══════════════════════════════════════════════════════
    // UI引用
    // ══════════════════════════════════════════════════════

    [Header("按钮")]
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    [Header("文本")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _versionText;

    // ══════════════════════════════════════════════════════
    // 事件（Presenter订阅）
    // ══════════════════════════════════════════════════════

    /// <summary>点击新游戏</summary>
    public event Action OnNewGameClicked;

    /// <summary>点击继续</summary>
    public event Action OnContinueClicked;

    /// <summary>点击设置</summary>
    public event Action OnSettingsClicked;

    /// <summary>点击退出</summary>
    public event Action OnQuitClicked;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    protected override void Awake()
    {
        base.Awake();

        // 绑定按钮事件
        if (_newGameButton != null)
            _newGameButton.onClick.AddListener(() => OnNewGameClicked?.Invoke());
        if (_continueButton != null)
            _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
        if (_settingsButton != null)
            _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
        if (_quitButton != null)
            _quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());

        // 显示版本号
        if (_versionText != null)
            _versionText.text = $"v{Application.version}";
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>设置「继续」按钮可用状态（有存档时才可用）</summary>
    public void SetContinueAvailable(bool available)
    {
        if (_continueButton != null)
        {
            _continueButton.interactable = available;
        }
    }

    /// <summary>设置标题文本</summary>
    public void SetTitle(string title)
    {
        if (_titleText != null)
            _titleText.text = title;
    }
}
