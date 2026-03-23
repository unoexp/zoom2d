// 📁 03_CoreSystems/SurvivalStatus/IStatusEffect.cs
using UnityEngine;


/// <summary>
/// 状态效果接口。所有临时状态（中毒、寒冷、饥饿加速等）均实现此接口。
/// 💡 扩展性设计：新增"生病"状态只需新建一个实现类，无需修改核心系统。
/// </summary>
public interface IStatusEffect
{
    string EffectId { get; }
    string DisplayName { get; }
    float Duration { get; }           // -1 = 永久，直到被治愈
    bool IsStackable { get; }
    
    void OnApply(SurvivalStatusSystem statusSystem);
    void OnTick(SurvivalStatusSystem statusSystem, float deltaTime);
    void OnRemove(SurvivalStatusSystem statusSystem);
}

// 使用示例：新增"冻伤"状态效果，无需修改任何现有代码
public class FrostbiteEffect : IStatusEffect
{
    public string EffectId => "effect_frostbite";
    public string DisplayName => "冻伤";
    public float Duration => 60f;
    public bool IsStackable => false;

    public void OnApply(SurvivalStatusSystem s) 
        => Debug.Log("冻伤已附加，体温流失速度加倍");
    
    public void OnTick(SurvivalStatusSystem s, float deltaTime)
        => s.ModifyAttribute(SurvivalAttributeType.Health, -2f * deltaTime);
    
    public void OnRemove(SurvivalStatusSystem s) 
        => Debug.Log("冻伤已解除");
}