// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Base/EventBus/Events/WorldEvents.cs
// 世界系统事件定义（昼夜循环、天气变化、环境事件）
// ══════════════════════════════════════════════════════════════════════

/// <summary>昼夜阶段变化事件</summary>
public struct DayPhaseChangedEvent : IEvent
{
    public DayPhase PreviousPhase;
    public DayPhase NewPhase;
    /// <summary>当前是第几天</summary>
    public int DayCount;
}

/// <summary>天气变化事件</summary>
public struct WeatherChangedEvent : IEvent
{
    public WeatherType PreviousWeather;
    public WeatherType NewWeather;
    /// <summary>天气强度 0~1（暴风雪/小雪等）</summary>
    public float Intensity;
}

/// <summary>游戏内时间更新事件（每分钟或关键时刻广播）</summary>
public struct GameTimeUpdatedEvent : IEvent
{
    /// <summary>当前游戏内小时 (0~23 或自定义周期)</summary>
    public float CurrentHour;
    /// <summary>当前天数</summary>
    public int DayCount;
    /// <summary>归一化时间 0~1（0=午夜，0.5=正午）</summary>
    public float NormalizedTime;
}

/// <summary>环境温度变化事件（天气/昼夜/区域共同影响后的最终值）</summary>
public struct AmbientTemperatureChangedEvent : IEvent
{
    /// <summary>环境温度（°C）</summary>
    public float Temperature;
    /// <summary>体感温度（考虑风寒/湿度后）</summary>
    public float FeelsLikeTemperature;
}

/// <summary>新的一天开始事件</summary>
public struct NewDayStartedEvent : IEvent
{
    public int DayCount;
}
