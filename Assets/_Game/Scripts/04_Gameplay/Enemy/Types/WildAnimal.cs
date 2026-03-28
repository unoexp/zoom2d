// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Enemy/Types/WildAnimal.cs
// 野生动物敌人。默认不攻击，被攻击后才会反击或逃跑。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 野生动物行为类型
/// </summary>
public enum AnimalBehavior
{
    Passive     = 0,    // 被动型：被攻击后逃跑（如兔子、鹿）
    Neutral     = 1,    // 中立型：被攻击后反击（如狼、野猪）
    Aggressive  = 2,    // 主动型：主动攻击玩家（如熊）
}

/// <summary>
/// 野生动物敌人。
///
/// 特殊行为：
///   · 三种行为模式：被动/中立/主动
///   · 被动型：受击后进入逃跑状态
///   · 中立型：受击后转为攻击状态，逃跑阈值更低
///   · 主动型：检测到玩家即追击
///   · 可通过猎杀获取食物和皮毛材料
///
/// 使用方式：
///   · 替换 EnemyBase 挂载在动物预制体上
///   · 通过 AnimalBehavior 配置不同动物性格
/// </summary>
public class WildAnimal : EnemyBase
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("动物特殊属性")]
    [Tooltip("行为类型")]
    [SerializeField] private AnimalBehavior _behaviorType = AnimalBehavior.Neutral;

    [Tooltip("受击后的警戒持续时间（秒）")]
    [SerializeField] private float _alertDuration = 10f;

    [Tooltip("游荡范围半径")]
    [SerializeField] private float _wanderRadius = 8f;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    private bool _isProvoked;
    private float _alertTimer;
    private Vector2 _homePosition;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    /// <summary>行为类型</summary>
    public AnimalBehavior BehaviorType => _behaviorType;

    /// <summary>是否被激怒</summary>
    public bool IsProvoked => _isProvoked;

    /// <summary>游荡范围半径</summary>
    public float WanderRadius => _wanderRadius;

    /// <summary>出生位置（游荡中心）</summary>
    public Vector2 HomePosition => _homePosition;

    /// <summary>是否应该主动攻击</summary>
    public bool ShouldAttackOnSight => _behaviorType == AnimalBehavior.Aggressive || _isProvoked;

    /// <summary>是否应该逃跑而非战斗</summary>
    public bool ShouldFleeWhenHit => _behaviorType == AnimalBehavior.Passive;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void OnEnable()
    {
        _homePosition = transform.position;
    }

    private void LateUpdate()
    {
        // 在 LateUpdate 中检查，避免隐藏基类的 Update/Start（FSM 驱动）
        if (_isProvoked && _behaviorType != AnimalBehavior.Aggressive)
        {
            _alertTimer -= Time.deltaTime;
            if (_alertTimer <= 0f)
            {
                _isProvoked = false;
            }
        }
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>触发警戒/激怒状态（被攻击时调用）</summary>
    public void Provoke()
    {
        if (_isProvoked) return;
        _isProvoked = true;
        _alertTimer = _alertDuration;

        if (_behaviorType == AnimalBehavior.Passive)
        {
            // 被动型：立刻逃跑
            FSM?.ChangeState(EnemyState.Flee);
        }
        else
        {
            // 中立型/主动型：转入追击
            FSM?.ChangeState(EnemyState.Chase);
        }
    }

    /// <summary>是否远离出生点（判断是否需要返回）</summary>
    public bool IsFarFromHome()
    {
        return Vector2.Distance(transform.position, _homePosition) > _wanderRadius * 1.5f;
    }
}
