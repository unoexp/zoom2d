// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Trading/Views/TradingPanelView.cs
// 交易面板View。显示NPC商品列表和交易操作。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 交易面板 View。
///
/// 核心职责：
///   · 显示NPC名称和玩家金币
///   · 显示购买/出售标签和商品列表
///   · 提供购买/出售按钮
///   · 绑定 ViewModel 事件驱动更新
/// </summary>
public class TradingPanelView : UIPanel
{
    // ══════════════════════════════════════════════════════
    // UI 引用
    // ══════════════════════════════════════════════════════

    [Header("顶部信息")]
    [SerializeField] private TextMeshProUGUI _merchantNameText;
    [SerializeField] private TextMeshProUGUI _playerGoldText;

    [Header("标签")]
    [SerializeField] private Button _buyTabButton;
    [SerializeField] private Button _sellTabButton;

    [Header("商品列表")]
    [SerializeField] private Transform _itemListContainer;
    [SerializeField] private GameObject _itemTemplate;

    [Header("选中详情")]
    [SerializeField] private TextMeshProUGUI _selectedItemName;
    [SerializeField] private TextMeshProUGUI _selectedItemPrice;
    [SerializeField] private Image _selectedItemIcon;

    [Header("操作")]
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _actionButtonText;
    [SerializeField] private Button _closeButton;

    // ══════════════════════════════════════════════════════
    // ViewModel
    // ══════════════════════════════════════════════════════

    private TradingViewModel _viewModel;

    /// <summary>标签切换</summary>
    public event System.Action<bool> OnTabSwitched;
    /// <summary>商品点击</summary>
    public event System.Action<int> OnItemClicked;
    /// <summary>执行交易</summary>
    public event System.Action OnActionClicked;
    /// <summary>关闭面板</summary>
    public event System.Action OnCloseClicked;

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    public void Bind(TradingViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnListUpdated -= RefreshList;
            _viewModel.OnSelectedItemChanged -= RefreshDetail;
            _viewModel.OnGoldUpdated -= RefreshGold;
        }

        _viewModel = viewModel;

        if (_viewModel != null)
        {
            _viewModel.OnListUpdated += RefreshList;
            _viewModel.OnSelectedItemChanged += RefreshDetail;
            _viewModel.OnGoldUpdated += RefreshGold;
        }
    }

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    protected override void Awake()
    {
        base.Awake();

        if (_itemTemplate != null)
            _itemTemplate.SetActive(false);

        if (_buyTabButton != null)
            _buyTabButton.onClick.AddListener(() => OnTabSwitched?.Invoke(false));
        if (_sellTabButton != null)
            _sellTabButton.onClick.AddListener(() => OnTabSwitched?.Invoke(true));
        if (_actionButton != null)
            _actionButton.onClick.AddListener(() => OnActionClicked?.Invoke());
        if (_closeButton != null)
            _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
    }

    private void OnDestroy()
    {
        Bind(null);
        if (_buyTabButton != null) _buyTabButton.onClick.RemoveAllListeners();
        if (_sellTabButton != null) _sellTabButton.onClick.RemoveAllListeners();
        if (_actionButton != null) _actionButton.onClick.RemoveAllListeners();
        if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
    }

    // ══════════════════════════════════════════════════════
    // 刷新方法
    // ══════════════════════════════════════════════════════

    private void RefreshList()
    {
        if (_viewModel == null || _itemListContainer == null) return;

        // 清理
        for (int i = _itemListContainer.childCount - 1; i >= 0; i--)
        {
            var child = _itemListContainer.GetChild(i);
            if (child.gameObject.activeSelf)
                Destroy(child.gameObject);
        }

        // 商家名称
        if (_merchantNameText != null)
            _merchantNameText.text = _viewModel.MerchantName;

        // 生成列表
        var list = _viewModel.CurrentList;
        if (_itemTemplate == null) return;

        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            var go = Instantiate(_itemTemplate, _itemListContainer);
            go.SetActive(true);

            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                string stockText = item.RemainingStock >= 0
                    ? $" [{item.RemainingStock}]" : "";
                text.text = $"{item.DisplayName}{stockText}  {item.GoldPrice}G";
                text.color = item.CanAfford ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            }

            int index = i;
            var button = go.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => OnItemClicked?.Invoke(index));
        }

        // 更新操作按钮文本
        if (_actionButtonText != null)
            _actionButtonText.text = _viewModel.ShowingSellTab ? "出售" : "购买";
    }

    private void RefreshDetail(TradeItemDisplayData data)
    {
        if (_selectedItemName != null)
            _selectedItemName.text = data.DisplayName ?? "";

        if (_selectedItemPrice != null)
            _selectedItemPrice.text = $"{data.GoldPrice} 金币";

        if (_selectedItemIcon != null)
        {
            _selectedItemIcon.sprite = data.Icon;
            _selectedItemIcon.enabled = data.Icon != null;
        }

        if (_actionButton != null)
            _actionButton.interactable = data.CanAfford;
    }

    private void RefreshGold(int gold)
    {
        if (_playerGoldText != null)
            _playerGoldText.text = $"金币: {gold}";
    }
}
