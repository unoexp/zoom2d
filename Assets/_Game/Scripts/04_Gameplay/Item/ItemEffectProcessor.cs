// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Item/ItemEffectProcessor.cs
// 物品使用效果处理器。监听物品使用事件，执行消耗品效果。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 物品效果处理器。
///
/// 核心职责：
///   · 监听 ItemUsedEvent，查找物品定义
///   · 对 ConsumableItemSO 执行效果（恢复生存属性、施加状态效果等）
///   · 通过 EventBus 发布效果结果，驱动 05_Show 的视觉反馈
///
/// 设计说明：
///   · 解耦道具逻辑：背包系统只管「使用」动作，本类处理「效果」
///   · 通过 IItemDataService 查找物品定义，不直接依赖具体 SO 子类
///   · 新增消耗品效果类型只需扩展 ProcessEffect 的 switch 分支
/// </summary>
public class ItemEffectProcessor : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 缓存引用
    // ══════════════════════════════════════════════════════

    private IItemDataService _itemDataService;
    private SurvivalStatusSystem _survivalSystem;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        ServiceLocator.Register<ItemEffectProcessor>(this);
    }

    private void Start()
    {
        _itemDataService = ServiceLocator.Get<IItemDataService>();
        _survivalSystem = ServiceLocator.Get<SurvivalStatusSystem>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<ItemUsedEvent>(OnItemUsed);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ItemUsedEvent>(OnItemUsed);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<ItemEffectProcessor>();
    }

    // ══════════════════════════════════════════════════════
    // 事件处理
    // ══════════════════════════════════════════════════════

    /// <summary>处理物品使用事件</summary>
    private void OnItemUsed(ItemUsedEvent evt)
    {
        if (_itemDataService == null || _survivalSystem == null) return;

        var itemDef = _itemDataService.GetItemDefinition(evt.ItemId);
        if (itemDef == null) return;

        // 只处理消耗品
        var consumable = itemDef as ConsumableItemSO;
        if (consumable == null) return;

        ProcessConsumable(consumable);
    }

    // ══════════════════════════════════════════════════════
    // 消耗品效果处理
    // ══════════════════════════════════════════════════════

    /// <summary>处理消耗品的所有效果</summary>
    private void ProcessConsumable(ConsumableItemSO consumable)
    {
        if (consumable.Effects == null) return;

        // [PERF] 使用 for 避免 foreach 的 enumerator GC
        for (int i = 0; i < consumable.Effects.Length; i++)
        {
            ProcessEffect(consumable.Effects[i]);
        }

        // 发布使用完成事件，通知 05_Show 播放反馈
        EventBus.Publish(new ConsumableUsedEvent
        {
            ItemId = consumable.ItemId,
            DisplayName = consumable.DisplayName,
            EffectCount = consumable.Effects.Length
        });
    }

    /// <summary>处理单条效果</summary>
    private void ProcessEffect(ConsumableEffect effect)
    {
        switch (effect.EffectType)
        {
            case ConsumableEffectType.RestoreHealth:
            case ConsumableEffectType.RestoreHunger:
            case ConsumableEffectType.RestoreThirst:
            case ConsumableEffectType.RestoreStamina:
            case ConsumableEffectType.RestoreTemperature:
                ProcessRestoreEffect(effect);
                break;

            case ConsumableEffectType.Buff:
            case ConsumableEffectType.Debuff:
                ProcessStatusEffect(effect);
                break;

            case ConsumableEffectType.CureEffect:
                ProcessCureEffect(effect);
                break;

            default:
                Debug.LogWarning($"[ItemEffectProcessor] 未知的消耗品效果类型: {effect.EffectType}");
                break;
        }
    }

    /// <summary>处理属性恢复效果</summary>
    private void ProcessRestoreEffect(ConsumableEffect effect)
    {
        if (effect.IsOverTime && effect.Duration > 0f)
        {
            // 持续效果：创建一个临时状态效果
            var regenEffect = new RegenStatusEffect(
                $"consumable_regen_{effect.AttributeType}",
                effect.AttributeType,
                effect.Value / effect.Duration, // 每秒恢复量
                effect.Duration
            );
            _survivalSystem.ApplyEffect(regenEffect);
        }
        else
        {
            // 瞬时效果：直接修改属性
            _survivalSystem.ModifyAttribute(effect.AttributeType, effect.Value);
        }
    }

    /// <summary>处理状态效果施加</summary>
    private void ProcessStatusEffect(ConsumableEffect effect)
    {
        if (string.IsNullOrEmpty(effect.StatusEffectId))
        {
            Debug.LogWarning("[ItemEffectProcessor] Buff/Debuff 缺少 StatusEffectId");
            return;
        }

        // 状态效果由外部系统注册和管理，此处仅发布请求事件
        EventBus.Publish(new StatusEffectRequestEvent
        {
            EffectId = effect.StatusEffectId,
            Duration = effect.Duration
        });
    }

    /// <summary>处理解除状态效果</summary>
    private void ProcessCureEffect(ConsumableEffect effect)
    {
        if (string.IsNullOrEmpty(effect.StatusEffectId))
        {
            Debug.LogWarning("[ItemEffectProcessor] CureEffect 缺少 StatusEffectId");
            return;
        }

        _survivalSystem.RemoveEffect(effect.StatusEffectId);
    }
}

// ══════════════════════════════════════════════════════════════════════
// 回复型状态效果（持续恢复属性）
// ══════════════════════════════════════════════════════════════════════

/// <summary>
/// 消耗品产生的持续恢复效果。
/// 每秒恢复指定量的生存属性，持续指定时间。
/// </summary>
public class RegenStatusEffect : IStatusEffect
{
    public string EffectId { get; private set; }
    public string DisplayName { get; private set; }
    public float Duration { get; private set; }
    public bool IsStackable => false;

    private readonly SurvivalAttributeType _attributeType;
    private readonly float _regenPerSecond;

    public RegenStatusEffect(string effectId, SurvivalAttributeType attributeType,
        float regenPerSecond, float duration)
    {
        EffectId = effectId;
        _attributeType = attributeType;
        _regenPerSecond = regenPerSecond;
        Duration = duration;
        DisplayName = $"{attributeType} 恢复";
    }

    public void OnApply(SurvivalStatusSystem owner) { }

    public void OnTick(SurvivalStatusSystem owner, float deltaTime)
    {
        owner.ModifyAttribute(_attributeType, _regenPerSecond * deltaTime);
    }

    public void OnRemove(SurvivalStatusSystem owner) { }
}
