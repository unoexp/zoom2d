// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Enemy/EnemyBase.cs
// 敌人基类。所有敌人类型继承此类，实现 IDamageable。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 敌人基类（MonoBehaviour）。
///
/// 核心职责：
///   · 持有 EnemyDefinitionSO 数据引用
///   · 实现 IDamageable（受伤/死亡）
///   · 驱动 EnemyStateMachine
///   · 管理运行时属性（血量、目标引用等）
///
/// 扩展方式：
///   · 特殊敌人类型继承此类，重写行为方法
///   · 通用敌人直接使用此类 + 不同的 EnemyDefinitionSO
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour, IDamageable, IPoolable
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("敌人数据")]
    [SerializeField] private EnemyDefinitionSO _definition;

    // ══════════════════════════════════════════════════════
    // 组件引用
    // ══════════════════════════════════════════════════════

    private Rigidbody2D _rb;
    private Animator _animator;
    private EnemyStateMachine _fsm;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    private float _currentHealth;
    private bool _isDead;
    private Transform _target; // 追击目标（通常是玩家）

    // ══════════════════════════════════════════════════════
    // IDamageable 实现
    // ══════════════════════════════════════════════════════

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _definition != null ? _definition.MaxHealth : 100f;
    public bool IsDead => _isDead;
    public Transform Transform => transform;

    // ══════════════════════════════════════════════════════
    // 公有属性（供 FSM 状态使用）
    // ══════════════════════════════════════════════════════

    public EnemyDefinitionSO Definition => _definition;
    public Rigidbody2D Rb => _rb;
    public Animator Anim => _animator;
    public Transform Target => _target;
    public EnemyStateMachine FSM => _fsm;

    /// <summary>与目标的距离（无目标返回 float.MaxValue）</summary>
    public float DistanceToTarget =>
        _target != null ? Vector2.Distance(transform.position, _target.position) : float.MaxValue;

    /// <summary>血量百分比 0~1</summary>
    public float HealthPercent => MaxHealth > 0 ? _currentHealth / MaxHealth : 0f;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _rb.freezeRotation = true;
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        _fsm?.Update(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        _fsm?.FixedUpdate(Time.fixedDeltaTime);
    }

    // ══════════════════════════════════════════════════════
    // 初始化
    // ══════════════════════════════════════════════════════

    /// <summary>初始化敌人状态（用于首次生成和对象池重用）</summary>
    public void Initialize()
    {
        _currentHealth = MaxHealth;
        _isDead = false;

        // 尝试获取玩家作为默认目标
        var player = GameObject.FindWithTag("Player");
        _target = player != null ? player.transform : null;

        // 创建状态机
        _fsm = new EnemyStateMachine(this);
        _fsm.Initialize(EnemyState.Idle);
    }

    // ══════════════════════════════════════════════════════
    // IDamageable 实现
    // ══════════════════════════════════════════════════════

    public void TakeDamage(DamageInfo info)
    {
        if (_isDead) return;

        _currentHealth -= info.Damage;
        _currentHealth = Mathf.Max(0f, _currentHealth);

        // 击退
        if (info.KnockbackForce > 0f && _rb != null)
        {
            _rb.AddForce(info.KnockbackDirection * info.KnockbackForce, ForceMode2D.Impulse);
        }

        if (_currentHealth <= 0f)
        {
            Die(info);
        }
    }

    public void Heal(float amount)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Min(_currentHealth + amount, MaxHealth);
    }

    // ══════════════════════════════════════════════════════
    // IPoolable 实现
    // ══════════════════════════════════════════════════════

    public void OnSpawn()
    {
        Initialize();
    }

    public void OnDespawn()
    {
        _target = null;
        _fsm = null;
    }

    // ══════════════════════════════════════════════════════
    // 公有方法（供 FSM 状态调用）
    // ══════════════════════════════════════════════════════

    /// <summary>设置追击目标</summary>
    public void SetTarget(Transform target) => _target = target;

    /// <summary>朝目标方向移动</summary>
    public void MoveTowardsTarget(float speedMultiplier = 1f)
    {
        if (_target == null || _definition == null) return;

        float dir = Mathf.Sign(_target.position.x - transform.position.x);
        _rb.velocity = new Vector2(dir * _definition.MoveSpeed * speedMultiplier, _rb.velocity.y);

        // 更新朝向
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dir > 0 ? 1 : -1);
        transform.localScale = scale;
    }

    /// <summary>停止移动</summary>
    public void StopMoving()
    {
        _rb.velocity = new Vector2(0f, _rb.velocity.y);
    }

    /// <summary>设置动画状态</summary>
    public void SetAnimationState(string stateName)
    {
        if (_animator != null)
            _animator.Play(stateName, 0, 0f);
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    private void Die(DamageInfo lastHit)
    {
        _isDead = true;
        _fsm?.ChangeState(EnemyState.Dead);
        StopMoving();

        // 发布死亡事件 → LootSystem 监听并生成掉落物
        int killerInstanceId = lastHit.Attacker != null ? lastHit.Attacker.GetInstanceID() : 0;
        EventBus.Publish(new EntityDiedEvent
        {
            EntityInstanceId = gameObject.GetInstanceID(),
            KillerInstanceId = killerInstanceId,
            Cause = DeathCause.Combat
        });

        Debug.Log($"[Enemy] {_definition?.DisplayName ?? name} 死亡");
    }
}
