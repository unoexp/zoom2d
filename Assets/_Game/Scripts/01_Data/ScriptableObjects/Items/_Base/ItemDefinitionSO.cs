// 📁 01_Data/ScriptableObjects/Items/_Base/ItemDefinitionSO.cs
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 所有物品的数据定义基类。纯数据，零运行时逻辑。
/// 💡 数据驱动设计核心：新增物品只需创建.asset文件，无需改代码。
/// </summary>
public abstract class ItemDefinitionSO : ScriptableObject
{
    [Header("基础信息")]
    public string ItemId;           // 全局唯一ID (用于存档/事件传递)
    public string DisplayName;
    [TextArea] public string Description;
    public Sprite Icon;
    public GameObject WorldPrefab;  // 世界中掉落时的Prefab (Addressable Key)
    
    [Header("背包属性")]
    public int MaxStackSize = 1;
    public float Weight = 0.1f;
    
    [Header("交互")]
    public bool IsPickupable = true;
    
    // 💡 扩展点：子类重写此方法定义使用效果，通过ItemEffectProcessor调用
    public virtual void OnUse(GameObject user) { }
    
    // 💡 扩展点：MOD可以通过重写此方法注入自定义逻辑
    public virtual bool CanUse(GameObject user) => true;
}

// 📁 01_Data/ScriptableObjects/Items/Consumable/ConsumableItemSO.cs
/// <summary>消耗品扩展：食物、药品等</summary>
[CreateAssetMenu(fileName = "Item_Consumable_", menuName = "SurvivalGame/Items/Consumable")]
public class ConsumableItemSO : ItemDefinitionSO
{
    [Header("消耗效果")]
    public float HungerRestore;
    public float ThirstRestore;
    public float HealthRestore;
    public float DurationSeconds;               // 持续效果时长（0=即时）
    // public List<StatusEffectData> StatusEffects; // 附加状态效果（如中毒、增益）
}