// 📁 02_Infrastructure/EventBus/IEvent.cs
using System;
using System.Collections.Generic;


/// <summary>
/// 所有事件的标记接口，使用结构体以避免GC
/// </summary>
public interface IEvent { }

// 📁 02_Infrastructure/EventBus/EventBus.cs
/// <summary>
/// 全局类型安全事件总线，基于泛型字典实现。
/// 使用结构体事件 + 静态访问，避免运行时GC压力。
/// </summary>
public static class EventBus
{
    // 每个事件类型对应一个独立的处理器注册表
    private static readonly Dictionary<Type, Delegate> _handlers 
        = new Dictionary<Type, Delegate>();

    /// <summary>订阅事件</summary>
    public static void Subscribe<T>(Action<T> handler) where T : struct, IEvent
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var existing))
            _handlers[type] = Delegate.Combine(existing, handler);
        else
            _handlers[type] = handler;
    }

    /// <summary>取消订阅（OnDestroy中务必调用）</summary>
    public static void Unsubscribe<T>(Action<T> handler) where T : struct, IEvent
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var existing))
        {
            var updated = Delegate.Remove(existing, handler);
            if (updated == null) _handlers.Remove(type);
            else _handlers[type] = updated;
        }
    }

    /// <summary>发布事件</summary>
    public static void Publish<T>(T evt) where T : struct, IEvent
    {
        if (_handlers.TryGetValue(typeof(T), out var handler))
            (handler as Action<T>)?.Invoke(evt);
    }

    /// <summary>清除所有订阅（场景切换时调用）</summary>
    public static void Clear() => _handlers.Clear();
}