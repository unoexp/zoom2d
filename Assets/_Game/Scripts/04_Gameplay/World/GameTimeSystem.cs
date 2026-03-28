// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/World/GameTimeSystem.cs
// 游戏时间系统门面。统一的时间查询接口，包装 DayNightCycle。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 游戏时间系统。
///
/// 核心职责：
///   · 包装 DayNightCycle，提供统一的时间查询 API
///   · 跟踪总游戏时间（秒）
///   · 提供时间格式化工具方法
///   · 注册到 ServiceLocator 供全局访问
///
/// 设计说明：
///   · 解决多处 TODO 对 IGameTimeSystem 的依赖
///   · DayNightCycle 管理昼夜循环细节，本类提供高层 API
/// </summary>
public class GameTimeSystem : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 缓存引用
    // ══════════════════════════════════════════════════════

    private DayNightCycle _dayNightCycle;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    /// <summary>游戏启动后的总时间（秒，不受暂停影响）</summary>
    private float _totalPlayTime;

    /// <summary>游戏内经过的总天数（含小数部分）</summary>
    private float _totalGameDays;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    /// <summary>当前游戏内小时（0~24）</summary>
    public float CurrentHour => _dayNightCycle != null ? _dayNightCycle.CurrentHour : 0f;

    /// <summary>当前天数</summary>
    public int DayCount => _dayNightCycle != null ? _dayNightCycle.DayCount : 1;

    /// <summary>当前昼夜阶段</summary>
    public DayPhase CurrentPhase => _dayNightCycle != null ? _dayNightCycle.CurrentPhase : DayPhase.Morning;

    /// <summary>归一化时间 0~1</summary>
    public float NormalizedTime => _dayNightCycle != null ? _dayNightCycle.NormalizedTime : 0f;

    /// <summary>总游玩时间（秒）</summary>
    public float TotalPlayTime => _totalPlayTime;

    /// <summary>是否为白天（Dawn~Dusk）</summary>
    public bool IsDaytime
    {
        get
        {
            var phase = CurrentPhase;
            return phase >= DayPhase.Dawn && phase <= DayPhase.Dusk;
        }
    }

    /// <summary>是否为夜晚（Night~Midnight）</summary>
    public bool IsNighttime => !IsDaytime;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        ServiceLocator.Register<GameTimeSystem>(this);
    }

    private void Start()
    {
        _dayNightCycle = ServiceLocator.Get<DayNightCycle>();
    }

    private void Update()
    {
        _totalPlayTime += Time.deltaTime;
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<GameTimeSystem>();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>获取格式化的时间字符串（如 "第3天 14:30"）</summary>
    public string GetFormattedTime()
    {
        float hour = CurrentHour;
        int h = (int)hour;
        int m = (int)((hour - h) * 60f);
        return $"第{DayCount}天 {h:D2}:{m:D2}";
    }

    /// <summary>获取格式化的游玩时长（如 "2h 15m"）</summary>
    public string GetFormattedPlayTime()
    {
        int totalMinutes = Mathf.FloorToInt(_totalPlayTime / 60f);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
    }

    /// <summary>检查游戏内是否经过了指定天数（用于 NPC 出现条件等）</summary>
    public bool HasPassedDays(int requiredDays)
    {
        return DayCount >= requiredDays;
    }

    /// <summary>设置总游玩时间（存档恢复用）</summary>
    public void SetTotalPlayTime(float time)
    {
        _totalPlayTime = time;
    }
}
