// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Base/ObjectPool/IPoolable.cs
// 对象池化接口。所有需要被 ObjectPoolManager 管理的对象实现此接口。
// ══════════════════════════════════════════════════════════════════════

/// <summary>
/// 可池化对象接口。
/// 挂载在需要池化的 Prefab 上，ObjectPoolManager 在取出/归还时自动调用。
/// </summary>
public interface IPoolable
{
    /// <summary>从池中取出时调用（替代 Awake/Start 的初始化入口）</summary>
    void OnSpawn();

    /// <summary>归还池中时调用（清理状态、重置数据）</summary>
    void OnDespawn();
}
