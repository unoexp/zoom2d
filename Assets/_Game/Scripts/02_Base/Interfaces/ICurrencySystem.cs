// 📁 02_Base/Interfaces/ICurrencySystem.cs
// 货币系统接口定义，供表现层通过ServiceLocator访问

/// <summary>
/// 货币系统接口
/// 🏗️ 定义在02_Base层，03_Core实现，05_Show引用
/// </summary>
public interface ICurrencySystem
{
    /// <summary>当前金币数量</summary>
    int Gold { get; }

    /// <summary>增加金币</summary>
    void AddGold(int amount, string reason = "");

    /// <summary>尝试消费金币（余额不足返回 false）</summary>
    bool TrySpendGold(int amount, string reason = "");

    /// <summary>检查是否有足够金币</summary>
    bool HasEnoughGold(int amount);
}
