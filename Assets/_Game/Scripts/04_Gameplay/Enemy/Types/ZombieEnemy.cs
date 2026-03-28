// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Enemy/Types/ZombieEnemy.cs
// 丧尸敌人。缓慢但耐打，受到伤害时进入狂暴状态。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 丧尸敌人。
///
/// 特殊行为：
///   · 血量低于 50% 时进入狂暴模式（移速和攻击力提升）
///   · 狂暴模式下不会逃跑（覆盖基类逃跑阈值）
///   · 死亡时掉落感染类物品的概率更高
///
/// 使用方式：
///   · 替换 EnemyBase 挂载在丧尸预制体上
///   · 配合 ZombieEnemy 专用的 EnemyDefinitionSO 资源
/// </summary>
public class ZombieEnemy : EnemyBase
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("丧尸特殊属性")]
    [Tooltip("狂暴触发的血量阈值（归一化 0~1）")]
    [SerializeField] private float _rageThreshold = 0.5f;

    [Tooltip("狂暴模式移速倍率")]
    [SerializeField] private float _rageSpeedMultiplier = 1.5f;

    [Tooltip("狂暴模式攻击力倍率")]
    [SerializeField] private float _rageDamageMultiplier = 1.3f;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    private bool _isEnraged;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    /// <summary>是否处于狂暴状态</summary>
    public bool IsEnraged => _isEnraged;

    /// <summary>当前移速倍率</summary>
    public float CurrentSpeedMultiplier => _isEnraged ? _rageSpeedMultiplier : 1f;

    /// <summary>当前攻击力倍率</summary>
    public float CurrentDamageMultiplier => _isEnraged ? _rageDamageMultiplier : 1f;

    // ══════════════════════════════════════════════════════
    // 重写
    // ══════════════════════════════════════════════════════

    private void LateUpdate()
    {
        // 在 LateUpdate 中检查，避免隐藏基类的 Update（FSM 驱动）
        if (!_isEnraged && !IsDead && HealthPercent <= _rageThreshold)
        {
            EnterRage();
        }
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    /// <summary>进入狂暴模式</summary>
    private void EnterRage()
    {
        _isEnraged = true;

        EventBus.Publish(new EnemyRageEvent
        {
            EntityInstanceId = gameObject.GetInstanceID(),
            EnemyId = Definition != null ? Definition.EnemyId : "",
            IsEnraged = true
        });

        Debug.Log($"[ZombieEnemy] {(Definition != null ? Definition.DisplayName : name)} 进入狂暴模式！");
    }
}

/// <summary>敌人狂暴状态变化事件（驱动视觉特效）</summary>
public struct EnemyRageEvent : IEvent
{
    public int EntityInstanceId;
    public string EnemyId;
    public bool IsEnraged;
}
