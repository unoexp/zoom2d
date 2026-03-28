// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Settings/SettingsPresenter.cs
// 设置面板Presenter。管理设置读写和应用。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 设置 Presenter。读取/保存设置，应用音量和画面选项。
/// 设置通过 PlayerPrefs 持久化。
/// </summary>
public class SettingsPresenter : MonoBehaviour
{
    [SerializeField] private SettingsPanelView _panelView;

    // PlayerPrefs 键
    private const string KEY_MASTER_VOL = "Settings_MasterVolume";
    private const string KEY_MUSIC_VOL = "Settings_MusicVolume";
    private const string KEY_SFX_VOL = "Settings_SFXVolume";
    private const string KEY_FULLSCREEN = "Settings_Fullscreen";

    // 当前值缓存
    private float _masterVolume = 1f;
    private float _musicVolume = 0.8f;
    private float _sfxVolume = 0.8f;
    private bool _fullscreen = true;

    private void Start()
    {
        // 加载保存的设置
        LoadSettings();

        if (_panelView != null)
        {
            _panelView.OnMasterVolumeChanged += v => _masterVolume = v;
            _panelView.OnMusicVolumeChanged += v => _musicVolume = v;
            _panelView.OnSFXVolumeChanged += v => _sfxVolume = v;
            _panelView.OnFullscreenChanged += v => _fullscreen = v;
            _panelView.OnApplyClicked += HandleApply;
            _panelView.OnBackClicked += HandleBack;

            var uiManager = ServiceLocator.Get<UIManager>();
            if (uiManager != null)
                uiManager.RegisterPanel(_panelView);
        }

        // 启动时应用设置
        ApplySettings();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>打开设置面板</summary>
    public void OpenSettings()
    {
        if (_panelView != null)
            _panelView.SetValues(_masterVolume, _musicVolume, _sfxVolume, _fullscreen);

        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null)
            uiManager.OpenPanel(_panelView);
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    private void HandleApply()
    {
        ApplySettings();
        SaveSettings();
    }

    private void HandleBack()
    {
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null)
            uiManager.ClosePanel(_panelView);
    }

    private void ApplySettings()
    {
        // 应用音量
        AudioListener.volume = _masterVolume;

        if (ServiceLocator.TryGet<AudioManager>(out var audioManager))
        {
            audioManager.SetVolume(AudioGroup.Music, _musicVolume);
            audioManager.SetVolume(AudioGroup.SFX, _sfxVolume);
        }

        // 应用全屏
        Screen.fullScreen = _fullscreen;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(KEY_MASTER_VOL, _masterVolume);
        PlayerPrefs.SetFloat(KEY_MUSIC_VOL, _musicVolume);
        PlayerPrefs.SetFloat(KEY_SFX_VOL, _sfxVolume);
        PlayerPrefs.SetInt(KEY_FULLSCREEN, _fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        _masterVolume = PlayerPrefs.GetFloat(KEY_MASTER_VOL, 1f);
        _musicVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 0.8f);
        _sfxVolume = PlayerPrefs.GetFloat(KEY_SFX_VOL, 0.8f);
        _fullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
    }
}
