// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/World/DayNightCycle.cs
// 昼夜循环系统。驱动游戏内时间流逝和昼夜阶段切换。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 昼夜循环系统。
///
/// 核心职责：
///   · 驱动游戏内时间流逝（可配置一天的实际时长）
///   · 根据时间自动切换 DayPhase 并通过 EventBus 广播
///   · 支持全局暂停和时间缩放（睡眠快进）
///   · 广播 GameTimeUpdatedEvent 供 UI 和其他系统使用
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("时间配置")]
    [Tooltip("一天的实际时长（秒）。默认 1860 秒 = 31 分钟")]
    [SerializeField] private float _dayDurationSeconds = 1860f;

    [Tooltip("游戏开始时的小时（0~24）")]
    [SerializeField] private float _startHour = 8f;

    [Header("阶段时间点（小时）")]
    [SerializeField] private float _dawnHour = 5f;
    [SerializeField] private float _morningHour = 7f;
    [SerializeField] private float _noonHour = 11f;
    [SerializeField] private float _afternoonHour = 14f;
    [SerializeField] private float _duskHour = 17f;
    [SerializeField] private float _nightHour = 20f;
    [SerializeField] private float _midnightHour = 0f;

    [Header("时间更新频率")]
    [Tooltip("GameTimeUpdatedEvent 广播间隔（游戏内分钟）")]
    [SerializeField] private float _timeUpdateIntervalMinutes = 1f;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    /// <summary>归一化时间 0~1（0=午夜 0:00，0.5=正午 12:00）</summary>
    private float _normalizedTime;

    /// <summary>当前天数（从1开始）</summary>
    private int _dayCount = 1;

    /// <summary>当前阶段</summary>
    private DayPhase _currentPhase = DayPhase.Morning;

    /// <summary>上次广播时间事件时的游戏内小时</summary>
    private float _lastBroadcastHour = -1f;

    /// <summary>时间缩放（睡眠快进时调大）</summary>
    private float _timeScale = 1f;

    /// <summary>是否暂停</summary>
    private bool _paused;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    /// <summary>当前游戏内小时（0~24）</summary>
    public float CurrentHour => _normalizedTime * 24f;

    /// <summary>当前天数</summary>
    public int DayCount => _dayCount;

    /// <summary>当前昼夜阶段</summary>
    public DayPhase CurrentPhase => _currentPhase;

    /// <summary>归一化时间</summary>
    public float NormalizedTime => _normalizedTime;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        ServiceLocator.Register<DayNightCycle>(this);
    }

    private void Start()
    {
        // 初始化时间
        _normalizedTime = _startHour / 24f;
        _currentPhase = EvaluatePhase(CurrentHour);
        _lastBroadcastHour = CurrentHour;

        EventBus.Publish(new DayPhaseChangedEvent
        {
            PreviousPhase = _currentPhase,
            NewPhase = _currentPhase,
            DayCount = _dayCount
        });
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<DayNightCycle>();
    }

    private void Update()
    {
        if (_paused) return;

        AdvanceTime(Time.deltaTime);
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>暂停时间流逝</summary>
    public void Pause() => _paused = true;

    /// <summary>恢复时间流逝</summary>
    public void Resume() => _paused = false;

    /// <summary>设置时间缩放（睡眠快进）</summary>
    public void SetTimeScale(float scale) => _timeScale = Mathf.Max(0f, scale);

    /// <summary>直接跳转到指定小时</summary>
    public void SkipToHour(float targetHour)
    {
        _normalizedTime = Mathf.Repeat(targetHour / 24f, 1f);
        CheckPhaseChange();
        BroadcastTimeUpdate();
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    private void AdvanceTime(float deltaTime)
    {
        float previousNormalized = _normalizedTime;

        // 每秒推进的归一化时间量
        float rate = (1f / _dayDurationSeconds) * _timeScale;
        _normalizedTime += rate * deltaTime;

        // 跨天检测
        if (_normalizedTime >= 1f)
        {
            _normalizedTime -= 1f;
            _dayCount++;

            EventBus.Publish(new NewDayStartedEvent { DayCount = _dayCount });
            Debug.Log($"[DayNight] 第 {_dayCount} 天开始");
        }

        CheckPhaseChange();
        CheckTimeBroadcast();
    }

    /// <summary>检测阶段切换</summary>
    private void CheckPhaseChange()
    {
        var newPhase = EvaluatePhase(CurrentHour);
        if (newPhase != _currentPhase)
        {
            var prev = _currentPhase;
            _currentPhase = newPhase;

            EventBus.Publish(new DayPhaseChangedEvent
            {
                PreviousPhase = prev,
                NewPhase = newPhase,
                DayCount = _dayCount
            });
        }
    }

    /// <summary>定期广播时间更新</summary>
    private void CheckTimeBroadcast()
    {
        float hourInterval = _timeUpdateIntervalMinutes / 60f;
        if (Mathf.Abs(CurrentHour - _lastBroadcastHour) >= hourInterval)
        {
            BroadcastTimeUpdate();
            _lastBroadcastHour = CurrentHour;
        }
    }

    private void BroadcastTimeUpdate()
    {
        EventBus.Publish(new GameTimeUpdatedEvent
        {
            CurrentHour = CurrentHour,
            DayCount = _dayCount,
            NormalizedTime = _normalizedTime
        });
    }

    /// <summary>根据小时判定当前阶段</summary>
    private DayPhase EvaluatePhase(float hour)
    {
        // 按时间顺序判定（从午夜开始）
        if (hour < _dawnHour)       return DayPhase.Midnight;
        if (hour < _morningHour)    return DayPhase.Dawn;
        if (hour < _noonHour)       return DayPhase.Morning;
        if (hour < _afternoonHour)  return DayPhase.Noon;
        if (hour < _duskHour)       return DayPhase.Afternoon;
        if (hour < _nightHour)      return DayPhase.Dusk;
        if (hour < 24f)             return DayPhase.Night;
        return DayPhase.Midnight;
    }
}
