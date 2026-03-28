// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/World/WeatherSystem.cs
// 天气系统。管理天气切换，影响环境温度和生存属性衰减。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 天气管理系统。
///
/// 核心职责：
///   · 管理当前天气类型和强度
///   · 根据天气计算环境温度修正
///   · 通过 EventBus 广播天气变化和环境温度
///   · 支持手动设置和随机天气变化
///
/// 设计说明：
///   · 天气影响 SurvivalStatusSystem 的衰减倍率
///     （通过 AddDecayMultiplier / RemoveDecayMultiplier）
///   · 天气变化事件驱动 05_Show 层的视觉/音效表现
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("基础温度")]
    [Tooltip("无天气影响时的基础环境温度（°C）")]
    [SerializeField] private float _baseTemperature = 25f;

    [Header("天气变化")]
    [Tooltip("天气自动变化的最短间隔（秒）")]
    [SerializeField] private float _minWeatherDuration = 120f;

    [Tooltip("天气自动变化的最长间隔（秒）")]
    [SerializeField] private float _maxWeatherDuration = 600f;

    [Tooltip("是否启用自动天气变化")]
    [SerializeField] private bool _autoWeatherChange = true;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    private WeatherType _currentWeather = WeatherType.Clear;
    private float _weatherIntensity = 0f;
    private float _weatherTimer;
    private float _nextWeatherChangeTime;
    private float _currentTemperature;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    public WeatherType CurrentWeather => _currentWeather;
    public float WeatherIntensity => _weatherIntensity;
    public float CurrentTemperature => _currentTemperature;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        ServiceLocator.Register<WeatherSystem>(this);
    }

    private void Start()
    {
        _currentTemperature = _baseTemperature;
        ScheduleNextWeatherChange();

        // 初始广播
        BroadcastTemperature();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<DayPhaseChangedEvent>(OnDayPhaseChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DayPhaseChangedEvent>(OnDayPhaseChanged);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<WeatherSystem>();
    }

    private void Update()
    {
        if (_autoWeatherChange)
        {
            _weatherTimer += Time.deltaTime;
            if (_weatherTimer >= _nextWeatherChangeTime)
            {
                RandomWeatherChange();
                ScheduleNextWeatherChange();
            }
        }

        UpdateTemperature();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// 手动设置天气。
    /// </summary>
    /// <param name="weather">天气类型</param>
    /// <param name="intensity">强度 0~1</param>
    public void SetWeather(WeatherType weather, float intensity = 1f)
    {
        if (weather == _currentWeather && Mathf.Approximately(intensity, _weatherIntensity))
            return;

        var prev = _currentWeather;
        _currentWeather = weather;
        _weatherIntensity = Mathf.Clamp01(intensity);

        EventBus.Publish(new WeatherChangedEvent
        {
            PreviousWeather = prev,
            NewWeather = weather,
            Intensity = _weatherIntensity
        });

        Debug.Log($"[Weather] 天气变化：{prev} → {weather}（强度 {_weatherIntensity:F1}）");
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    /// <summary>更新环境温度（基础温度 + 天气修正 + 昼夜修正）</summary>
    private void UpdateTemperature()
    {
        float tempModifier = GetWeatherTemperatureModifier();
        float newTemp = _baseTemperature + tempModifier;

        // 温度变化超过阈值时广播
        if (Mathf.Abs(newTemp - _currentTemperature) > 0.5f)
        {
            _currentTemperature = newTemp;
            BroadcastTemperature();
        }
    }

    /// <summary>根据天气类型返回温度修正值</summary>
    private float GetWeatherTemperatureModifier()
    {
        float intensity = _weatherIntensity;

        switch (_currentWeather)
        {
            case WeatherType.Clear:        return 0f;
            case WeatherType.Cloudy:       return -2f * intensity;
            case WeatherType.Foggy:        return -3f * intensity;
            case WeatherType.Rainy:        return -5f * intensity;
            case WeatherType.Thunderstorm: return -7f * intensity;
            case WeatherType.Snowy:        return -15f * intensity;
            case WeatherType.Blizzard:     return -25f * intensity;
            case WeatherType.Heatwave:     return 10f * intensity;
            case WeatherType.AcidRain:     return -3f * intensity;
            case WeatherType.RadiationStorm: return 2f * intensity;
            default: return 0f;
        }
    }

    private void BroadcastTemperature()
    {
        EventBus.Publish(new AmbientTemperatureChangedEvent
        {
            Temperature = _currentTemperature,
            FeelsLikeTemperature = _currentTemperature + GetWindChillModifier()
        });
    }

    /// <summary>风寒效应（暴风雪/雷暴时体感更冷）</summary>
    private float GetWindChillModifier()
    {
        switch (_currentWeather)
        {
            case WeatherType.Blizzard:     return -5f * _weatherIntensity;
            case WeatherType.Thunderstorm: return -2f * _weatherIntensity;
            default: return 0f;
        }
    }

    /// <summary>随机天气变化</summary>
    private void RandomWeatherChange()
    {
        // 简单随机：大部分时间晴天，小概率恶劣天气
        float roll = Random.value;
        WeatherType newWeather;
        float intensity;

        if (roll < 0.35f)      { newWeather = WeatherType.Clear;   intensity = 0f; }
        else if (roll < 0.55f) { newWeather = WeatherType.Cloudy;  intensity = Random.Range(0.3f, 0.8f); }
        else if (roll < 0.65f) { newWeather = WeatherType.Foggy;   intensity = Random.Range(0.4f, 1f); }
        else if (roll < 0.80f) { newWeather = WeatherType.Rainy;   intensity = Random.Range(0.3f, 1f); }
        else if (roll < 0.88f) { newWeather = WeatherType.Thunderstorm; intensity = Random.Range(0.5f, 1f); }
        else if (roll < 0.93f) { newWeather = WeatherType.Snowy;   intensity = Random.Range(0.3f, 0.9f); }
        else if (roll < 0.96f) { newWeather = WeatherType.Heatwave; intensity = Random.Range(0.5f, 1f); }
        else                   { newWeather = WeatherType.Blizzard; intensity = Random.Range(0.6f, 1f); }

        SetWeather(newWeather, intensity);
    }

    private void ScheduleNextWeatherChange()
    {
        _weatherTimer = 0f;
        _nextWeatherChangeTime = Random.Range(_minWeatherDuration, _maxWeatherDuration);
    }

    /// <summary>昼夜阶段变化时调整基础温度</summary>
    private void OnDayPhaseChanged(DayPhaseChangedEvent evt)
    {
        // 根据昼夜阶段微调基础温度
        switch (evt.NewPhase)
        {
            case DayPhase.Dawn:      _baseTemperature = 18f; break;
            case DayPhase.Morning:   _baseTemperature = 22f; break;
            case DayPhase.Noon:      _baseTemperature = 30f; break;
            case DayPhase.Afternoon: _baseTemperature = 28f; break;
            case DayPhase.Dusk:      _baseTemperature = 22f; break;
            case DayPhase.Night:     _baseTemperature = 15f; break;
            case DayPhase.Midnight:  _baseTemperature = 12f; break;
        }
    }
}
