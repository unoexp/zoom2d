// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Building/Views/BuildingPanelView.cs
// 建造面板View。显示可建造列表和详情。
// ══════════════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 建造面板View。继承 UIPanel。
/// </summary>
public class BuildingPanelView : UIPanel
{
    // ══════════════════════════════════════════════════════
    // UI引用
    // ══════════════════════════════════════════════════════

    [Header("建筑列表")]
    [SerializeField] private Transform _listContainer;
    [SerializeField] private GameObject _listItemPrefab;

    [Header("建筑详情")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descText;
    [SerializeField] private Transform _materialListContainer;
    [SerializeField] private GameObject _materialItemPrefab;

    [Header("操作")]
    [SerializeField] private Button _buildButton;
    [SerializeField] private TextMeshProUGUI _buildButtonText;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private float _resultDuration = 2f;

    // ══════════════════════════════════════════════════════
    // 事件
    // ══════════════════════════════════════════════════════

    public event Action<int> OnBuildingSelected;
    public event Action OnBuildClicked;

    // ══════════════════════════════════════════════════════
    // 运行时
    // ══════════════════════════════════════════════════════

    private BuildingViewModel _viewModel;
    private float _resultTimer;
    private readonly List<GameObject> _listInstances = new List<GameObject>();
    private readonly List<GameObject> _materialInstances = new List<GameObject>();

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    public void Bind(BuildingViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnBuildingListUpdated -= HandleListUpdated;
            _viewModel.OnSelectedBuildingChanged -= HandleSelectionChanged;
            _viewModel.OnBuildResult -= HandleBuildResult;
        }

        _viewModel = viewModel;

        if (_viewModel != null)
        {
            _viewModel.OnBuildingListUpdated += HandleListUpdated;
            _viewModel.OnSelectedBuildingChanged += HandleSelectionChanged;
            _viewModel.OnBuildResult += HandleBuildResult;
        }
    }

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    protected override void Awake()
    {
        base.Awake();
        if (_buildButton != null)
            _buildButton.onClick.AddListener(() => OnBuildClicked?.Invoke());
        if (_resultText != null)
            _resultText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_resultTimer > 0f)
        {
            _resultTimer -= Time.unscaledDeltaTime;
            if (_resultTimer <= 0f && _resultText != null)
                _resultText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        Bind(null);
    }

    // ══════════════════════════════════════════════════════
    // ViewModel 回调
    // ══════════════════════════════════════════════════════

    private void HandleListUpdated(List<BuildingDisplayData> buildings)
    {
        ClearInstances(_listInstances);
        if (_listContainer == null || _listItemPrefab == null) return;

        for (int i = 0; i < buildings.Count; i++)
        {
            var instance = Instantiate(_listItemPrefab, _listContainer);
            _listInstances.Add(instance);

            var text = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = buildings[i].IsBuilt
                    ? $"✓ {buildings[i].DisplayName}"
                    : buildings[i].DisplayName;
                text.color = buildings[i].IsBuilt ? Color.gray
                    : buildings[i].CanBuild ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            }

            int index = i;
            var button = instance.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = !buildings[i].IsBuilt;
                button.onClick.AddListener(() => OnBuildingSelected?.Invoke(index));
            }
        }
    }

    private void HandleSelectionChanged(BuildingDisplayData data)
    {
        if (_nameText != null) _nameText.text = data.DisplayName;
        if (_descText != null) _descText.text = data.Description;
        if (_buildButton != null) _buildButton.interactable = data.CanBuild && !data.IsBuilt;
        if (_buildButtonText != null)
            _buildButtonText.text = data.IsBuilt ? "已建造" : data.CanBuild ? "建造" : "材料不足";

        // 材料列表
        ClearInstances(_materialInstances);
        if (_materialListContainer != null && _materialItemPrefab != null && data.Materials != null)
        {
            for (int i = 0; i < data.Materials.Length; i++)
            {
                var instance = Instantiate(_materialItemPrefab, _materialListContainer);
                _materialInstances.Add(instance);
                var text = instance.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    var mat = data.Materials[i];
                    text.text = $"{mat.DisplayName}  {mat.CurrentAmount}/{mat.RequiredAmount}";
                    text.color = mat.IsSatisfied ? Color.white : Color.red;
                }
            }
        }
    }

    private void HandleBuildResult(CraftingResult result, string name)
    {
        if (_resultText == null) return;
        _resultText.text = result == CraftingResult.Success ? $"建造完成：{name}" : "建造失败";
        _resultText.color = result == CraftingResult.Success ? Color.green : Color.red;
        _resultText.gameObject.SetActive(true);
        _resultTimer = _resultDuration;
    }

    private void ClearInstances(List<GameObject> instances)
    {
        for (int i = 0; i < instances.Count; i++)
            if (instances[i] != null) Destroy(instances[i]);
        instances.Clear();
    }
}
