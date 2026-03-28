// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Trading/ViewModels/TradingViewModel.cs
// 交易面板ViewModel。管理交易列表和选中交易的显示数据。
// ══════════════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 交易条目显示数据
/// </summary>
public struct TradeItemDisplayData
{
    public int StockIndex;
    public string ItemId;
    public string DisplayName;
    public Sprite Icon;
    public int GoldPrice;
    public int RemainingStock;  // -1=无限
    public bool IsSellingToPlayer;
    public bool CanAfford;
}

/// <summary>
/// 交易面板 ViewModel。
///
/// 核心职责：
///   · 持有当前NPC的出售和收购列表
///   · 管理选中条目状态
///   · 暴露事件通知 View 更新
/// </summary>
public class TradingViewModel
{
    // ══════════════════════════════════════════════════════
    // 数据
    // ══════════════════════════════════════════════════════

    private string _merchantName = string.Empty;
    private string _offerId = string.Empty;
    private readonly List<TradeItemDisplayData> _sellingItems = new List<TradeItemDisplayData>();
    private readonly List<TradeItemDisplayData> _buyingItems = new List<TradeItemDisplayData>();
    private bool _showingSellTab;
    private int _selectedIndex = -1;
    private int _playerGold;

    // ══════════════════════════════════════════════════════
    // 事件
    // ══════════════════════════════════════════════════════

    /// <summary>交易列表刷新</summary>
    public event Action OnListUpdated;

    /// <summary>选中条目变化</summary>
    public event Action<TradeItemDisplayData> OnSelectedItemChanged;

    /// <summary>玩家金币更新</summary>
    public event Action<int> OnGoldUpdated;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    public string MerchantName => _merchantName;
    public string OfferId => _offerId;
    public IReadOnlyList<TradeItemDisplayData> SellingItems => _sellingItems;
    public IReadOnlyList<TradeItemDisplayData> BuyingItems => _buyingItems;
    public bool ShowingSellTab => _showingSellTab;
    public int SelectedIndex => _selectedIndex;
    public int PlayerGold => _playerGold;

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>设置交易数据</summary>
    public void SetTradeData(string merchantName, string offerId,
                              List<TradeItemDisplayData> selling,
                              List<TradeItemDisplayData> buying,
                              int playerGold)
    {
        _merchantName = merchantName;
        _offerId = offerId;
        _playerGold = playerGold;

        _sellingItems.Clear();
        _buyingItems.Clear();

        if (selling != null)
            for (int i = 0; i < selling.Count; i++)
                _sellingItems.Add(selling[i]);

        if (buying != null)
            for (int i = 0; i < buying.Count; i++)
                _buyingItems.Add(buying[i]);

        _showingSellTab = false;
        _selectedIndex = -1;

        OnGoldUpdated?.Invoke(_playerGold);
        OnListUpdated?.Invoke();

        if (CurrentList.Count > 0)
            SelectItem(0);
    }

    /// <summary>切换标签（购买/出售）</summary>
    public void SwitchTab(bool showSellTab)
    {
        _showingSellTab = showSellTab;
        _selectedIndex = -1;
        OnListUpdated?.Invoke();

        if (CurrentList.Count > 0)
            SelectItem(0);
    }

    /// <summary>选中指定索引</summary>
    public void SelectItem(int index)
    {
        var list = CurrentList;
        if (index < 0 || index >= list.Count) return;
        _selectedIndex = index;
        OnSelectedItemChanged?.Invoke(list[index]);
    }

    /// <summary>更新玩家金币</summary>
    public void UpdateGold(int gold)
    {
        _playerGold = gold;
        OnGoldUpdated?.Invoke(gold);
    }

    /// <summary>当前显示的列表</summary>
    public IReadOnlyList<TradeItemDisplayData> CurrentList =>
        _showingSellTab ? (IReadOnlyList<TradeItemDisplayData>)_buyingItems : _sellingItems;

    /// <summary>清空数据</summary>
    public void Clear()
    {
        _sellingItems.Clear();
        _buyingItems.Clear();
        _merchantName = string.Empty;
        _offerId = string.Empty;
        _selectedIndex = -1;
    }
}
