// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Camera/CameraController.cs
// 2D跟随摄像机。平滑跟随玩家，支持前瞻和边界限制。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 2D跟随摄像机控制器。
///
/// 核心特性：
///   · 平滑跟随（可配置阻尼）
///   · 移动前瞻（摄像机偏向玩家移动方向）
///   · 可选边界限制（防止摄像机超出地图）
///   · 支持摄像机震动（受伤/爆炸反馈）
/// </summary>
public class CameraController : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("跟随目标")]
    [SerializeField] private Transform _target;

    [Header("跟随参数")]
    [Tooltip("跟随平滑速度")]
    [SerializeField] private float _smoothSpeed = 8f;

    [Tooltip("垂直偏移（摄像机比玩家高一点）")]
    [SerializeField] private float _verticalOffset = 1f;

    [Header("前瞻")]
    [Tooltip("水平前瞻距离")]
    [SerializeField] private float _lookAheadDistance = 2f;

    [Tooltip("前瞻平滑速度")]
    [SerializeField] private float _lookAheadSmooth = 4f;

    [Header("边界限制")]
    [SerializeField] private bool _useBounds;
    [SerializeField] private float _minX = -50f;
    [SerializeField] private float _maxX = 50f;
    [SerializeField] private float _minY = -10f;
    [SerializeField] private float _maxY = 30f;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    private float _currentLookAhead;
    private float _targetLookAhead;
    private Vector3 _shakeOffset;
    private float _shakeTimer;
    private float _shakeIntensity;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        ServiceLocator.Register<CameraController>(this);
    }

    private void Start()
    {
        if (_target == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) _target = player.transform;
        }

        // 初始位置直接对准目标
        if (_target != null)
        {
            Vector3 pos = _target.position;
            pos.y += _verticalOffset;
            pos.z = transform.position.z;
            transform.position = pos;
        }
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<CameraController>();
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        UpdateLookAhead();
        UpdateShake();
        FollowTarget();
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>设置跟随目标</summary>
    public void SetTarget(Transform target) => _target = target;

    /// <summary>
    /// 触发摄像机震动。
    /// </summary>
    /// <param name="intensity">震动强度</param>
    /// <param name="duration">持续时间（秒）</param>
    public void Shake(float intensity = 0.3f, float duration = 0.2f)
    {
        _shakeIntensity = intensity;
        _shakeTimer = duration;
    }

    /// <summary>设置边界</summary>
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        _useBounds = true;
        _minX = minX;
        _maxX = maxX;
        _minY = minY;
        _maxY = maxY;
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    private void FollowTarget()
    {
        Vector3 targetPos = _target.position;
        targetPos.x += _currentLookAhead;
        targetPos.y += _verticalOffset;
        targetPos.z = transform.position.z;

        // 平滑跟随
        Vector3 smoothed = Vector3.Lerp(transform.position, targetPos, _smoothSpeed * Time.deltaTime);

        // 加上震动
        smoothed += _shakeOffset;

        // 边界限制
        if (_useBounds)
        {
            smoothed.x = Mathf.Clamp(smoothed.x, _minX, _maxX);
            smoothed.y = Mathf.Clamp(smoothed.y, _minY, _maxY);
        }

        transform.position = smoothed;
    }

    private void UpdateLookAhead()
    {
        // 根据目标 localScale.x 判断朝向
        _targetLookAhead = _target.localScale.x > 0
            ? _lookAheadDistance
            : -_lookAheadDistance;

        _currentLookAhead = Mathf.Lerp(_currentLookAhead, _targetLookAhead,
            _lookAheadSmooth * Time.deltaTime);
    }

    private void UpdateShake()
    {
        if (_shakeTimer > 0f)
        {
            _shakeTimer -= Time.deltaTime;
            _shakeOffset = Random.insideUnitCircle * _shakeIntensity;
        }
        else
        {
            _shakeOffset = Vector3.zero;
        }
    }
}
