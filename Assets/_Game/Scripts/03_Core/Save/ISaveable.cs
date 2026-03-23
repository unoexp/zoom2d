// 📁 03_CoreSystems/Save/ISaveable.cs
/// <summary>
/// 凡需要持久化的系统均实现此接口。
/// SaveLoadSystem 在存档时遍历所有注册的 ISaveable。
/// </summary>
public interface ISaveable
{
    /// <summary>唯一存档ID，建议用 nameof(类名)</summary>
    string SaveKey { get; }
    
    /// <summary>将当前状态序列化为数据对象</summary>
    object CaptureState();
    
    /// <summary>从数据对象恢复状态</summary>
    void RestoreState(object state);
}