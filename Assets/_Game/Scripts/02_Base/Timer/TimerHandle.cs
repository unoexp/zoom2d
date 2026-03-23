// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Infrastructure/Timer/TimerHandle.cs
// 计时器句柄 —— 外部持有此对象来控制/查询计时器
// ══════════════════════════════════════════════════════════════════════
using System;
using UnityEngine;

/// <summary>
/// 计时器句柄，由 TimerSystem.Create() 返回。
/// 外部通过句柄暂停、恢复、取消、查询计时器，
/// 不直接操作内部 Timer 对象，符合封装原则。
/// </summary>
public sealed class TimerHandle
{
    // ── 只读属性 ──────────────────────────────────────────
    
    /// <summary>计时器唯一 ID</summary>
    public uint Id { get; internal set; }
    
    /// <summary>计时器是否仍然有效（未取消、未完成）</summary>
    public bool IsValid => TimerSystem.Instance != null 
                        && TimerSystem.Instance.IsValid(this);
    
    /// <summary>剩余时间（秒）</summary>
    public float RemainingTime => TimerSystem.Instance != null
                               ? TimerSystem.Instance.GetRemainingTime(this)
                               : 0f;
    
    /// <summary>已经过时间（秒）</summary>
    public float ElapsedTime => TimerSystem.Instance != null
                             ? TimerSystem.Instance.GetElapsedTime(this)
                             : 0f;

    /// <summary>[0, 1] 归一化进度，用于进度条 UI</summary>
    public float Progress => TimerSystem.Instance != null
                          ? TimerSystem.Instance.GetProgress(this)
                          : 0f;

    // ── 控制方法 ──────────────────────────────────────────

    public void Pause()   => TimerSystem.Instance?.Pause(this);
    public void Resume()  => TimerSystem.Instance?.Resume(this);
    public void Cancel()  => TimerSystem.Instance?.Cancel(this);
}

