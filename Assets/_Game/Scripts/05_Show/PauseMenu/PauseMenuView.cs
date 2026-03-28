// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/PauseMenu/PauseMenuView.cs
// 暂停菜单面板View。
// ══════════════════════════════════════════════════════════════════════
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 暂停菜单面板View。
/// 全屏面板，打开时暂停游戏时间。
/// </summary>
public class PauseMenuView : UIPanel
{
    // ══════════════════════════════════════════════════════
    // UI引用
    // ══════════════════════════════════════════════════════

    [Header("按钮")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _mainMenuButton;

    [Header("状态提示")]
    [SerializeField] private TextMeshProUGUI _saveStatusText;
    [SerializeField] private float _saveStatusDuration = 2f;

    // ══════════════════════════════════════════════════════
    // 事件
    // ══════════════════════════════════════════════════════

    public event Action OnResumeClicked;
    public event Action OnSaveClicked;
    public event Action OnSettingsClicked;
    public event Action OnMainMenuClicked;

    // ══════════════════════════════════════════════════════
    // 运行时
    // ══════════════════════════════════════════════════════

    private float _saveStatusTimer;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    protected override void Awake()
    {
        base.Awake();

        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
        if (_saveButton != null)
            _saveButton.onClick.AddListener(() => OnSaveClicked?.Invoke());
        if (_settingsButton != null)
            _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());

        if (_saveStatusText != null)
            _saveStatusText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_saveStatusTimer > 0f)
        {
            _saveStatusTimer -= Time.unscaledDeltaTime;
            if (_saveStatusTimer <= 0f && _saveStatusText != null)
                _saveStatusText.gameObject.SetActive(false);
        }
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>显示存档状态提示</summary>
    public void ShowSaveStatus(string message, Color color)
    {
        if (_saveStatusText == null) return;
        _saveStatusText.text = message;
        _saveStatusText.color = color;
        _saveStatusText.gameObject.SetActive(true);
        _saveStatusTimer = _saveStatusDuration;
    }

    /// <summary>设置存档按钮可用状态</summary>
    public void SetSaveAvailable(bool available)
    {
        if (_saveButton != null)
            _saveButton.interactable = available;
    }
}
