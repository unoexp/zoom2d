// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Base/Audio/AudioManager.cs
// 音频管理器。统一管理 BGM、音效、环境音的播放和音量控制。
// ══════════════════════════════════════════════════════════════════════
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 中央音频管理系统。
///
/// 核心特性：
///   · 分组音量控制（Master/Music/SFX/Ambient/UI/Voice）
///   · BGM 淡入淡出切换
///   · SFX 对象池化播放（避免 AudioSource 泛滥）
///   · 通过 MonoSingleton + ServiceLocator 双重访问
/// </summary>
public sealed class AudioManager : MonoSingleton<AudioManager>
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("音效目录")]
    [Tooltip("音效目录 SO 列表（可多个，会合并）")]
    [SerializeField] private AudioCatalogSO[] _catalogs;

    [Header("音频源")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _ambientSource;

    [Header("SFX 池")]
    [Tooltip("SFX AudioSource 对象池大小")]
    [SerializeField] private int _sfxPoolSize = 8;

    [Header("默认音量")]
    [SerializeField] [Range(0f, 1f)] private float _defaultMasterVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float _defaultMusicVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float _defaultSfxVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float _defaultAmbientVolume = 0.5f;

    // ══════════════════════════════════════════════════════
    // 字段
    // ══════════════════════════════════════════════════════

    /// <summary>各组音量</summary>
    private readonly Dictionary<AudioGroup, float> _volumes = new Dictionary<AudioGroup, float>();

    /// <summary>SFX AudioSource 池</summary>
    private readonly List<AudioSource> _sfxPool = new List<AudioSource>();

    /// <summary>音效ID → 音效条目（快速查找）</summary>
    private readonly Dictionary<string, AudioEntry> _entryMap = new Dictionary<string, AudioEntry>();

    /// <summary>BGM 淡入淡出状态</summary>
    private AudioClip _pendingMusic;
    private float _fadeTimer;
    private float _fadeDuration;
    private bool _isFading;
    private bool _isFadingOut;

    // ══════════════════════════════════════════════════════
    // 初始化
    // ══════════════════════════════════════════════════════

    protected override void OnInitialize()
    {
        // 初始化音量
        _volumes[AudioGroup.Master] = _defaultMasterVolume;
        _volumes[AudioGroup.Music] = _defaultMusicVolume;
        _volumes[AudioGroup.SFX] = _defaultSfxVolume;
        _volumes[AudioGroup.Ambient] = _defaultAmbientVolume;
        _volumes[AudioGroup.UI] = 1f;
        _volumes[AudioGroup.Voice] = 1f;

        // 自动创建音频源
        if (_musicSource == null)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
        }

        if (_ambientSource == null)
        {
            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.playOnAwake = false;
        }

        // 预热 SFX 池
        for (int i = 0; i < _sfxPoolSize; i++)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _sfxPool.Add(source);
        }

        // 加载音效目录
        LoadCatalogs();

        ServiceLocator.Register<AudioManager>(this);
    }

    protected override void OnDestroy()
    {
        ServiceLocator.Unregister<AudioManager>();
        base.OnDestroy();
    }

    private void Update()
    {
        if (_isFading)
            UpdateFade();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— 音量控制
    // ══════════════════════════════════════════════════════

    /// <summary>设置指定组的音量</summary>
    public void SetVolume(AudioGroup group, float volume)
    {
        _volumes[group] = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    /// <summary>获取指定组的音量</summary>
    public float GetVolume(AudioGroup group)
    {
        return _volumes.TryGetValue(group, out float vol) ? vol : 1f;
    }

    /// <summary>获取最终音量（组音量 × Master）</summary>
    public float GetEffectiveVolume(AudioGroup group)
    {
        float master = _volumes.TryGetValue(AudioGroup.Master, out float m) ? m : 1f;
        float groupVol = _volumes.TryGetValue(group, out float g) ? g : 1f;
        return master * groupVol;
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— BGM
    // ══════════════════════════════════════════════════════

    /// <summary>播放背景音乐（带淡入淡出）</summary>
    public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
    {
        if (clip == null) return;

        if (_musicSource.isPlaying && fadeDuration > 0f)
        {
            // 先淡出当前曲目，再淡入新曲目
            _pendingMusic = clip;
            _fadeDuration = fadeDuration;
            _fadeTimer = 0f;
            _isFading = true;
            _isFadingOut = true;
        }
        else
        {
            _musicSource.clip = clip;
            _musicSource.volume = GetEffectiveVolume(AudioGroup.Music);
            _musicSource.Play();
        }
    }

    /// <summary>停止背景音乐</summary>
    public void StopMusic(float fadeDuration = 1f)
    {
        if (fadeDuration > 0f && _musicSource.isPlaying)
        {
            _pendingMusic = null;
            _fadeDuration = fadeDuration;
            _fadeTimer = 0f;
            _isFading = true;
            _isFadingOut = true;
        }
        else
        {
            _musicSource.Stop();
        }
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— SFX
    // ══════════════════════════════════════════════════════

    /// <summary>播放一次性音效</summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        var source = GetAvailableSfxSource();
        if (source == null) return;

        source.clip = clip;
        source.volume = GetEffectiveVolume(AudioGroup.SFX) * volumeScale;
        source.Play();
    }

    /// <summary>在指定位置播放3D音效（2D游戏中用于左右声道定位）</summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, GetEffectiveVolume(AudioGroup.SFX) * volumeScale);
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— 环境音
    // ══════════════════════════════════════════════════════

    /// <summary>播放环境音（循环）</summary>
    public void PlayAmbient(AudioClip clip)
    {
        if (clip == null) return;
        _ambientSource.clip = clip;
        _ambientSource.volume = GetEffectiveVolume(AudioGroup.Ambient);
        _ambientSource.Play();
    }

    /// <summary>停止环境音</summary>
    public void StopAmbient()
    {
        _ambientSource.Stop();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API —— 通过ID播放
    // ══════════════════════════════════════════════════════

    /// <summary>通过音效ID播放（自动判断分组）</summary>
    /// <param name="audioId">音效目录中注册的ID</param>
    public void Play(string audioId)
    {
        if (!_entryMap.TryGetValue(audioId, out var entry))
        {
            Debug.LogWarning($"[AudioManager] 未注册的音效ID: {audioId}");
            return;
        }

        var clip = GetRandomClip(entry);
        if (clip == null) return;

        float volumeScale = entry.VolumeScale > 0f ? entry.VolumeScale : 1f;

        switch (entry.Group)
        {
            case AudioGroup.Music:
                PlayMusic(clip);
                break;
            case AudioGroup.Ambient:
                PlayAmbient(clip);
                break;
            default:
                PlaySFXWithPitch(clip, volumeScale, entry.PitchMin, entry.PitchMax);
                break;
        }
    }

    /// <summary>通过音效ID在指定位置播放</summary>
    public void PlayAtPosition(string audioId, Vector3 position)
    {
        if (!_entryMap.TryGetValue(audioId, out var entry))
        {
            Debug.LogWarning($"[AudioManager] 未注册的音效ID: {audioId}");
            return;
        }

        var clip = GetRandomClip(entry);
        if (clip == null) return;

        float volumeScale = entry.VolumeScale > 0f ? entry.VolumeScale : 1f;
        PlaySFXAtPosition(clip, position, volumeScale);
    }

    /// <summary>检查音效ID是否已注册</summary>
    public bool HasAudio(string audioId)
    {
        return _entryMap.ContainsKey(audioId);
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    /// <summary>加载所有音效目录</summary>
    private void LoadCatalogs()
    {
        if (_catalogs == null) return;

        for (int i = 0; i < _catalogs.Length; i++)
        {
            var catalog = _catalogs[i];
            if (catalog == null || catalog.Entries == null) continue;

            for (int j = 0; j < catalog.Entries.Length; j++)
            {
                var entry = catalog.Entries[j];
                if (string.IsNullOrEmpty(entry.AudioId)) continue;
                if (entry.Clips == null || entry.Clips.Length == 0) continue;

                // 默认音量和音高
                if (entry.VolumeScale <= 0f) entry.VolumeScale = 1f;
                if (entry.PitchMin <= 0f) entry.PitchMin = 1f;
                if (entry.PitchMax <= 0f) entry.PitchMax = 1f;

                _entryMap[entry.AudioId] = entry;
            }
        }

        Debug.Log($"[AudioManager] 已加载 {_entryMap.Count} 条音效");
    }

    /// <summary>从条目中随机选取一个 AudioClip</summary>
    private static AudioClip GetRandomClip(AudioEntry entry)
    {
        if (entry.Clips == null || entry.Clips.Length == 0) return null;
        if (entry.Clips.Length == 1) return entry.Clips[0];
        return entry.Clips[Random.Range(0, entry.Clips.Length)];
    }

    /// <summary>播放带音高变化的音效</summary>
    private void PlaySFXWithPitch(AudioClip clip, float volumeScale, float pitchMin, float pitchMax)
    {
        if (clip == null) return;

        var source = GetAvailableSfxSource();
        if (source == null) return;

        source.clip = clip;
        source.volume = GetEffectiveVolume(AudioGroup.SFX) * volumeScale;
        source.pitch = pitchMin < pitchMax ? Random.Range(pitchMin, pitchMax) : 1f;
        source.Play();
    }

    private void ApplyVolumes()
    {
        _musicSource.volume = GetEffectiveVolume(AudioGroup.Music);
        _ambientSource.volume = GetEffectiveVolume(AudioGroup.Ambient);
    }

    /// <summary>从池中获取空闲的 AudioSource</summary>
    private AudioSource GetAvailableSfxSource()
    {
        // [PERF] 直接遍历，池大小固定且很小
        for (int i = 0; i < _sfxPool.Count; i++)
        {
            if (!_sfxPool[i].isPlaying)
                return _sfxPool[i];
        }

        // 所有都在播放，抢占最早的
        return _sfxPool[0];
    }

    /// <summary>BGM 淡入淡出更新</summary>
    private void UpdateFade()
    {
        _fadeTimer += Time.unscaledDeltaTime;
        float halfDuration = _fadeDuration * 0.5f;

        if (_isFadingOut)
        {
            float t = Mathf.Clamp01(_fadeTimer / halfDuration);
            _musicSource.volume = Mathf.Lerp(GetEffectiveVolume(AudioGroup.Music), 0f, t);

            if (_fadeTimer >= halfDuration)
            {
                _musicSource.Stop();

                if (_pendingMusic != null)
                {
                    _musicSource.clip = _pendingMusic;
                    _musicSource.Play();
                    _pendingMusic = null;
                    _isFadingOut = false;
                    _fadeTimer = 0f;
                }
                else
                {
                    _isFading = false;
                }
            }
        }
        else
        {
            // 淡入
            float t = Mathf.Clamp01(_fadeTimer / halfDuration);
            _musicSource.volume = Mathf.Lerp(0f, GetEffectiveVolume(AudioGroup.Music), t);

            if (_fadeTimer >= halfDuration)
                _isFading = false;
        }
    }
}
