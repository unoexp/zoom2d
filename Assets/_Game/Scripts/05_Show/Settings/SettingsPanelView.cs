// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Settings/SettingsPanelView.cs
// 设置面板View。音量、难度等游戏设置。
// ══════════════════════════════════════════════════════════════════════
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 设置面板View。继承 UIPanel。
/// </summary>
public class SettingsPanelView : UIPanel
{
    // ══════════════════════════════════════════════════════
    // UI引用
    // ══════════════════════════════════════════════════════

    [Header("音量设置")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI _masterValueText;
    [SerializeField] private TextMeshProUGUI _musicValueText;
    [SerializeField] private TextMeshProUGUI _sfxValueText;

    [Header("画面设置")]
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private TMP_Dropdown _resolutionDropdown;

    [Header("操作")]
    [SerializeField] private Button _applyButton;
    [SerializeField] private Button _backButton;

    // ══════════════════════════════════════════════════════
    // 事件
    // ══════════════════════════════════════════════════════

    public event Action<float> OnMasterVolumeChanged;
    public event Action<float> OnMusicVolumeChanged;
    public event Action<float> OnSFXVolumeChanged;
    public event Action<bool> OnFullscreenChanged;
    public event Action OnApplyClicked;
    public event Action OnBackClicked;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    protected override void Awake()
    {
        base.Awake();

        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.onValueChanged.AddListener(v =>
            {
                UpdateVolumeText(_masterValueText, v);
                OnMasterVolumeChanged?.Invoke(v);
            });
        }

        if (_musicVolumeSlider != null)
        {
            _musicVolumeSlider.onValueChanged.AddListener(v =>
            {
                UpdateVolumeText(_musicValueText, v);
                OnMusicVolumeChanged?.Invoke(v);
            });
        }

        if (_sfxVolumeSlider != null)
        {
            _sfxVolumeSlider.onValueChanged.AddListener(v =>
            {
                UpdateVolumeText(_sfxValueText, v);
                OnSFXVolumeChanged?.Invoke(v);
            });
        }

        if (_fullscreenToggle != null)
            _fullscreenToggle.onValueChanged.AddListener(v => OnFullscreenChanged?.Invoke(v));

        if (_applyButton != null)
            _applyButton.onClick.AddListener(() => OnApplyClicked?.Invoke());

        if (_backButton != null)
            _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>初始化滑块值</summary>
    public void SetValues(float master, float music, float sfx, bool fullscreen)
    {
        if (_masterVolumeSlider != null) _masterVolumeSlider.SetValueWithoutNotify(master);
        if (_musicVolumeSlider != null) _musicVolumeSlider.SetValueWithoutNotify(music);
        if (_sfxVolumeSlider != null) _sfxVolumeSlider.SetValueWithoutNotify(sfx);
        if (_fullscreenToggle != null) _fullscreenToggle.SetIsOnWithoutNotify(fullscreen);

        UpdateVolumeText(_masterValueText, master);
        UpdateVolumeText(_musicValueText, music);
        UpdateVolumeText(_sfxValueText, sfx);
    }

    // ══════════════════════════════════════════════════════
    // 内部
    // ══════════════════════════════════════════════════════

    private void UpdateVolumeText(TextMeshProUGUI text, float value)
    {
        if (text != null)
            text.text = $"{Mathf.RoundToInt(value * 100)}%";
    }
}
