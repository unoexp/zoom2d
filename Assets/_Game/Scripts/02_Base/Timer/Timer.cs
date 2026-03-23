// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Infrastructure/Timer/Timer.cs
// 内部计时器数据对象（对象池复用，外部不可见）
// ══════════════════════════════════════════════════════════════════════

using System;


/// <summary>
/// 内部计时器数据体，由对象池管理。
/// 外部不直接访问，通过 TimerHandle 交互。
/// </summary>
internal sealed class Timer
{
    // ── 配置数据（Create 时写入）─────────────────────────
    internal uint     Id;
    internal float    Duration;          // 单次时长（秒）
    internal float    InitialDuration;   // 用于计算 Progress
    internal bool     IsLoop;            // 是否循环
    internal int      LoopCount;         // 循环次数（-1 = 无限）
    internal int      LoopRemaining;     // 剩余循环次数
    internal bool     UseRealTime;       // true = 不受时间缩放影响
    internal Action   OnComplete;        // 每次完成回调
    internal Action   OnLoopEnd;         // 所有循环结束回调

    // ── 运行时状态 ───────────────────────────────────────
    internal float    Elapsed;           // 当前周期已过时间
    internal bool     IsPaused;
    internal bool     IsFinished;        // 标记为待回收

    // ── 对象池复用：重置所有字段 ─────────────────────────
    internal void Reset()
    {
        Id              = 0;
        Duration        = 0f;
        InitialDuration = 0f;
        IsLoop          = false;
        LoopCount       = 1;
        LoopRemaining   = 1;
        UseRealTime     = false;
        OnComplete      = null;
        OnLoopEnd       = null;
        Elapsed         = 0f;
        IsPaused        = false;
        IsFinished      = false;
    }
}
