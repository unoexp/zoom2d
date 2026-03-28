// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/HUD/Views/SurvivalStatusBarView.cs
// 单条生存属性进度条View组件。纯显示，无业务逻辑。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 单条生存属性进度条View。
///
/// 职责：
///   · 根据 ViewModel 数据渲染进度条填充、颜色、数值文本
///   · 根据预警等级切换颜色和闪烁动画
///   · 不直接访问业务系统，仅被动显示数据
/// </summary>
public class SurvivalStatusBarView : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("属性配置")]
    [SerializeField] private SurvivalAttributeType _attributeType;

    [Header("UI引用")]
    [SerializeField] private Image _fillImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _valueText;

    [Header("颜色配置")]
    [SerializeField] private Color _normalColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color _warningColor = new Color(0.9f, 0.7f, 0.1f);
    [SerializeField] private Color _dangerColor = new Color(0.9f, 0.2f, 0.1f);

    [Header("闪烁配置")]
    [SerializeField] private float _flashSpeed = 3f;

    // ══════════════════════════════════════════════════════
    // 运行时状态
    // ══════════════════════════════════════════════════════

    private int _currentWarningLevel = -1;
    private float _flashTimer;
    private bool _isFlashing;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    public SurvivalAttributeType AttributeType => _attributeType;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Update()
    {
        if (_isFlashing)
        {
            UpdateFlash();
        }
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>更新进度条显示</summary>
    public void UpdateDisplay(AttributeDisplayData data)
    {
        if (data.Type != _attributeType) return;

        // 更新填充量
        if (_fillImage != null)
        {
            _fillImage.fillAmount = data.Normalized;
        }

        // 更新数值文本
        if (_valueText != null)
        {
            _valueText.text = $"{(int)data.CurrentValue}/{(int)data.MaxValue}";
        }

        // 根据归一化值更新颜色
        UpdateBarColor(data.Normalized);
    }

    /// <summary>设置预警状态</summary>
    public void SetWarningLevel(int warningLevel)
    {
        _currentWarningLevel = warningLevel;
        _isFlashing = warningLevel >= (int)CriticalWarningLevel.Danger;

        if (!_isFlashing)
        {
            _flashTimer = 0f;
        }
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    /// <summary>根据归一化值渐变颜色</summary>
    private void UpdateBarColor(float normalized)
    {
        if (_fillImage == null) return;

        if (normalized > 0.5f)
        {
            _fillImage.color = _normalColor;
        }
        else if (normalized > 0.2f)
        {
            // 在 normal 和 warning 之间插值
            float t = (normalized - 0.2f) / 0.3f;
            _fillImage.color = Color.Lerp(_warningColor, _normalColor, t);
        }
        else
        {
            // 在 warning 和 danger 之间插值
            float t = normalized / 0.2f;
            _fillImage.color = Color.Lerp(_dangerColor, _warningColor, t);
        }
    }

    /// <summary>更新闪烁效果</summary>
    private void UpdateFlash()
    {
        _flashTimer += Time.unscaledDeltaTime * _flashSpeed;
        float alpha = 0.5f + 0.5f * Mathf.Sin(_flashTimer * Mathf.PI * 2f);

        if (_fillImage != null)
        {
            var color = _fillImage.color;
            color.a = alpha;
            _fillImage.color = color;
        }
    }
}
