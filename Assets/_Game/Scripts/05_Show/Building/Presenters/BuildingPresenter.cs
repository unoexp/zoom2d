// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Building/Presenters/BuildingPresenter.cs
// 建造界面Presenter。连接 BuildingSystem 与 View。
// ══════════════════════════════════════════════════════════════════════
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 建造界面 Presenter。
/// </summary>
public class BuildingPresenter : MonoBehaviour
{
    [SerializeField] private BuildingPanelView _panelView;

    private BuildingViewModel _viewModel;
    private BuildingSystem _buildingSystem;
    private IInventorySystem _inventorySystem;

    private void Awake()
    {
        _viewModel = new BuildingViewModel();
    }

    private void Start()
    {
        _buildingSystem = ServiceLocator.Get<BuildingSystem>();
        _inventorySystem = ServiceLocator.Get<IInventorySystem>();

        if (_panelView != null)
        {
            _panelView.Bind(_viewModel);
            _panelView.OnBuildingSelected += HandleBuildingSelected;
            _panelView.OnBuildClicked += HandleBuildClicked;

            var uiManager = ServiceLocator.Get<UIManager>();
            if (uiManager != null)
                uiManager.RegisterPanel(_panelView);
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<BuildCompletedEvent>(OnBuildCompleted);
        EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BuildCompletedEvent>(OnBuildCompleted);
        EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
    }

    private void OnDestroy()
    {
        if (_panelView != null)
        {
            _panelView.OnBuildingSelected -= HandleBuildingSelected;
            _panelView.OnBuildClicked -= HandleBuildClicked;
        }
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    public void OpenBuildingPanel()
    {
        RefreshList();
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null && _panelView != null)
            uiManager.OpenPanel(_panelView);
    }

    // ══════════════════════════════════════════════════════
    // 数据转换
    // ══════════════════════════════════════════════════════

    private void RefreshList()
    {
        if (_buildingSystem == null) return;

        var unlocked = _buildingSystem.GetUnlockedBuildings();
        var displayList = new List<BuildingDisplayData>(unlocked.Count);

        for (int i = 0; i < unlocked.Count; i++)
        {
            displayList.Add(ConvertToDisplay(unlocked[i]));
        }

        _viewModel.SetBuildings(displayList);
    }

    private BuildingDisplayData ConvertToDisplay(BuildingDefinitionSO def)
    {
        var data = new BuildingDisplayData
        {
            BuildingId = def.BuildingId,
            DisplayName = def.DisplayName,
            Description = def.Description,
            Category = def.Category,
            IsBuilt = _buildingSystem.IsBuilt(def.BuildingId),
            CanBuild = _buildingSystem.ValidateBuild(def.BuildingId) == CraftingResult.Success,
        };

        if (def.RequiredMaterials != null)
        {
            data.Materials = new BuildingMaterialDisplay[def.RequiredMaterials.Length];
            for (int i = 0; i < def.RequiredMaterials.Length; i++)
            {
                var mat = def.RequiredMaterials[i];
                string itemId = mat.Item != null ? mat.Item.ItemId : "";
                int current = _inventorySystem != null && !string.IsNullOrEmpty(itemId)
                    ? _inventorySystem.GetTotalItemCount(itemId) : 0;

                data.Materials[i] = new BuildingMaterialDisplay
                {
                    ItemId = itemId,
                    DisplayName = mat.Item != null ? mat.Item.DisplayName : "未知",
                    RequiredAmount = mat.Amount,
                    CurrentAmount = current,
                    IsSatisfied = current >= mat.Amount
                };
            }
        }

        return data;
    }

    // ══════════════════════════════════════════════════════
    // 交互处理
    // ══════════════════════════════════════════════════════

    private void HandleBuildingSelected(int index)
    {
        _viewModel.SelectBuilding(index);
    }

    private void HandleBuildClicked()
    {
        var selected = _viewModel.SelectedBuilding;
        if (!selected.HasValue || selected.Value.IsBuilt) return;

        var result = _buildingSystem.Build(selected.Value.BuildingId);
        _viewModel.NotifyBuildResult(result, selected.Value.DisplayName);
    }

    private void OnBuildCompleted(BuildCompletedEvent evt)
    {
        RefreshList();
    }

    private void OnInventoryChanged(InventoryChangedEvent evt)
    {
        if (_panelView != null && _panelView.IsVisible)
            RefreshList();
    }
}
