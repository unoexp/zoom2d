// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/01_Data/ScriptableObjects/Trading/TradeOfferSO.cs
// 交易报价数据定义。定义NPC可出售/收购的物品和价格。
// 💡 新增交易报价只需创建 .asset 文件，无需改代码。
// ══════════════════════════════════════════════════════════════════════
using System;
using UnityEngine;

/// <summary>
/// 单条交易条目（NPC出售或收购）
/// </summary>
[Serializable]
public struct TradeItem
{
    [Tooltip("交易物品")]
    public ItemDefinitionSO Item;

    [Tooltip("金币价格")]
    public int GoldPrice;

    [Tooltip("库存数量（-1=无限）")]
    public int Stock;

    [Tooltip("是否为NPC出售（false=NPC收购）")]
    public bool IsSellingToPlayer;
}

/// <summary>
/// 交易报价 ScriptableObject。
/// 定义一个NPC的完整交易清单。
/// </summary>
[CreateAssetMenu(fileName = "TradeOffer_", menuName = "SurvivalGame/Trading/Trade Offer")]
public class TradeOfferSO : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("报价ID（与NPCID关联）")]
    public string OfferId;

    [Tooltip("NPC显示名称")]
    public string MerchantName;

    [Header("出售清单（NPC卖给玩家）")]
    public TradeItem[] SellingItems;

    [Header("收购清单（NPC从玩家买）")]
    public TradeItem[] BuyingItems;

    [Header("条件")]
    [Tooltip("所需信任度")]
    [Range(0, 100)]
    public int RequiredTrust = 0;
}
