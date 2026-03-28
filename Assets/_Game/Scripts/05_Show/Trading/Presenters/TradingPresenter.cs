// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Trading/Presenters/TradingPresenter.cs
// 交易面板Presenter。连接TradingSystem与交易UI。
// ══════════════════════════════════════════════════════════════════════
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 交易面板 Presenter。
///
/// 核心职责：
///   · 订阅 TradeOpenRequestEvent 打开交易面板
///   · 从 TradingSystem 构建商品列表写入 ViewModel
///   · 处理购买/出售操作
///   · 金币变化时刷新 ViewModel
/// </summary>
public class TradingPresenter : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 引用
    // ══════════════════════════════════════════════════════

    [SerializeField] private TradingPanelView _view;

    private TradingViewModel _viewModel;
    private TradingSystem _tradingSystem;
    private ICurrencySystem _currencySystem;
    private IItemDataService _itemDataService;

    /// <summary>当前打开的报价ID</summary>
    private string _currentOfferId;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        _viewModel = new TradingViewModel();
    }

    private void Start()
    {
        _tradingSystem = ServiceLocator.Get<TradingSystem>();
        _currencySystem = ServiceLocator.Get<ICurrencySystem>();
        _itemDataService = ServiceLocator.Get<IItemDataService>();

        if (_view != null)
        {
            _view.Bind(_viewModel);

            var uiManager = ServiceLocator.Get<UIManager>();
            if (uiManager != null)
            {
                uiManager.RegisterPanel(_view);
            }

            _view.OnTabSwitched += OnTabSwitched;
            _view.OnItemClicked += OnItemClicked;
            _view.OnActionClicked += OnActionClicked;
            _view.OnCloseClicked += OnCloseClicked;
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<TradeOpenRequestEvent>(OnTradeOpenRequest);
        EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
        EventBus.Subscribe<TradeExecutedEvent>(OnTradeExecuted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TradeOpenRequestEvent>(OnTradeOpenRequest);
        EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
        EventBus.Unsubscribe<TradeExecutedEvent>(OnTradeExecuted);
    }

    private void OnDestroy()
    {
        if (_view != null)
        {
            _view.OnTabSwitched -= OnTabSwitched;
            _view.OnItemClicked -= OnItemClicked;
            _view.OnActionClicked -= OnActionClicked;
            _view.OnCloseClicked -= OnCloseClicked;

            var uiManager = ServiceLocator.Get<UIManager>();
            if (uiManager != null)
            {
                uiManager.UnregisterPanel(_view);
            }
        }
    }

    // ══════════════════════════════════════════════════════
    // 事件处理
    // ══════════════════════════════════════════════════════

    private void OnTradeOpenRequest(TradeOpenRequestEvent evt)
    {
        _currentOfferId = evt.OfferId;
        RefreshTradeData(evt.OfferId);

        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null && _view != null)
        {
            uiManager.OpenPanel(_view);
        }
    }

    private void OnCurrencyChanged(CurrencyChangedEvent evt)
    {
        _viewModel.UpdateGold(evt.NewAmount);
    }

    private void OnTradeExecuted(TradeExecutedEvent evt)
    {
        // 交易完成后刷新列表（库存可能变化）
        if (!string.IsNullOrEmpty(_currentOfferId))
        {
            RefreshTradeData(_currentOfferId);
        }
    }

    // ══════════════════════════════════════════════════════
    // View 交互处理
    // ══════════════════════════════════════════════════════

    private void OnTabSwitched(bool showSellTab)
    {
        _viewModel.SwitchTab(showSellTab);
    }

    private void OnItemClicked(int index)
    {
        _viewModel.SelectItem(index);
    }

    private void OnActionClicked()
    {
        if (string.IsNullOrEmpty(_currentOfferId)) return;
        if (_tradingSystem == null) return;

        int selectedIndex = _viewModel.SelectedIndex;
        if (selectedIndex < 0) return;

        var list = _viewModel.CurrentList;
        if (selectedIndex >= list.Count) return;

        var item = list[selectedIndex];

        if (_viewModel.ShowingSellTab)
        {
            // 出售
            _tradingSystem.SellItem(_currentOfferId, item.ItemId, 1);
        }
        else
        {
            // 购买
            _tradingSystem.BuyItem(_currentOfferId, item.StockIndex, 1);
        }
    }

    private void OnCloseClicked()
    {
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null && _view != null)
        {
            uiManager.ClosePanel(_view);
        }

        if (!string.IsNullOrEmpty(_currentOfferId))
        {
            EventBus.Publish(new TradeClosedEvent { NPCId = _currentOfferId });
        }

        _currentOfferId = null;
        _viewModel.Clear();
    }

    // ══════════════════════════════════════════════════════
    // 数据构建
    // ══════════════════════════════════════════════════════

    private void RefreshTradeData(string offerId)
    {
        if (_tradingSystem == null) return;

        var offer = _tradingSystem.GetOffer(offerId);
        if (offer == null) return;

        var stock = _tradingSystem.GetStock(offerId);
        if (stock == null) return;

        int playerGold = _currencySystem != null ? _currencySystem.Gold : 0;

        var selling = new List<TradeItemDisplayData>();
        var buying = new List<TradeItemDisplayData>();

        for (int i = 0; i < stock.Count; i++)
        {
            var item = stock[i];
            var displayData = BuildDisplayData(item, i, playerGold);

            if (item.IsSellingToPlayer)
                selling.Add(displayData);
            else
                buying.Add(displayData);
        }

        _viewModel.SetTradeData(offer.MerchantName, offerId,
                                 selling, buying, playerGold);
    }

    private TradeItemDisplayData BuildDisplayData(TradeItemRuntime item, int stockIndex, int playerGold)
    {
        string displayName = item.ItemId;
        Sprite icon = null;

        if (_itemDataService != null)
        {
            var itemDef = _itemDataService.GetItemDefinition(item.ItemId);
            if (itemDef != null)
            {
                displayName = itemDef.DisplayName;
                icon = itemDef.Icon;
            }
        }

        bool canAfford = item.IsSellingToPlayer
            ? playerGold >= item.GoldPrice && (item.RemainingStock == -1 || item.RemainingStock > 0)
            : true; // 出售总是可以的（只要玩家有物品）

        return new TradeItemDisplayData
        {
            StockIndex = stockIndex,
            ItemId = item.ItemId,
            DisplayName = displayName,
            Icon = icon,
            GoldPrice = item.GoldPrice,
            RemainingStock = item.RemainingStock,
            IsSellingToPlayer = item.IsSellingToPlayer,
            CanAfford = canAfford
        };
    }
}
