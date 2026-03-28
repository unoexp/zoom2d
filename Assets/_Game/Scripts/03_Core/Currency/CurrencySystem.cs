// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/03_Core/Currency/CurrencySystem.cs
// 货币系统。管理玩家金币的增减和查询。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 中央货币管理系统。
///
/// 核心职责：
///   · 管理玩家金币余额
///   · 增加/消费金币并广播事件
///   · 通过 ISaveable 支持存档
///   · 同时发布 PlayerGoldChangedEvent 兼容已有订阅
///
/// 设计说明：
///   · 通过 ServiceLocator 注册 ICurrencySystem 接口和 CurrencySystem 具体类
///   · 金币不允许为负数
///   · 消费失败时发布 CurrencyInsufficientEvent
/// </summary>
public class CurrencySystem : MonoBehaviour, ICurrencySystem, ISaveable
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("初始金币")]
    [SerializeField] private int _startingGold = 0;

    // ══════════════════════════════════════════════════════
    // 字段
    // ══════════════════════════════════════════════════════

    private int _gold;

    // ══════════════════════════════════════════════════════
    // ISaveable
    // ══════════════════════════════════════════════════════

    public string SaveKey => nameof(CurrencySystem);

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    public int Gold => _gold;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        _gold = _startingGold;
        ServiceLocator.Register<ICurrencySystem>(this);
        ServiceLocator.Register<CurrencySystem>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<ICurrencySystem>();
        ServiceLocator.Unregister<CurrencySystem>();
    }

    // ══════════════════════════════════════════════════════
    // ICurrencySystem 实现
    // ══════════════════════════════════════════════════════

    public void AddGold(int amount, string reason = "")
    {
        if (amount <= 0) return;

        int oldAmount = _gold;
        _gold += amount;

        PublishChanged(oldAmount, _gold, amount, reason);
        Debug.Log($"[Currency] +{amount} 金币 ({reason})，当前: {_gold}");
    }

    public bool TrySpendGold(int amount, string reason = "")
    {
        if (amount <= 0) return true;

        if (_gold < amount)
        {
            EventBus.Publish(new CurrencyInsufficientEvent
            {
                Required = amount,
                Current = _gold
            });
            return false;
        }

        int oldAmount = _gold;
        _gold -= amount;

        PublishChanged(oldAmount, _gold, -amount, reason);
        Debug.Log($"[Currency] -{amount} 金币 ({reason})，当前: {_gold}");
        return true;
    }

    public bool HasEnoughGold(int amount)
    {
        return _gold >= amount;
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    private void PublishChanged(int oldAmount, int newAmount, int delta, string reason)
    {
        EventBus.Publish(new CurrencyChangedEvent
        {
            OldAmount = oldAmount,
            NewAmount = newAmount,
            Delta = delta,
            Reason = reason
        });

        // 兼容已有的 PlayerGoldChangedEvent 订阅
        EventBus.Publish(new PlayerGoldChangedEvent
        {
            CurrentGold = newAmount,
            Delta = delta
        });
    }

    // ══════════════════════════════════════════════════════
    // ISaveable
    // ══════════════════════════════════════════════════════

    public object CaptureState()
    {
        return _gold;
    }

    public void RestoreState(object state)
    {
        if (state is int gold)
        {
            _gold = gold;
        }
    }
}
