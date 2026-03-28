// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/Building/ViewModels/BuildingViewModel.cs
// 建造界面的ViewModel。
// ══════════════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;

/// <summary>
/// 单条建筑的UI展示数据
/// </summary>
public struct BuildingDisplayData
{
    public string BuildingId;
    public string DisplayName;
    public string Description;
    public ShelterModuleCategory Category;
    public bool CanBuild;
    public bool IsBuilt;
    public BuildingMaterialDisplay[] Materials;
}

/// <summary>
/// 建造材料展示数据
/// </summary>
public struct BuildingMaterialDisplay
{
    public string ItemId;
    public string DisplayName;
    public int RequiredAmount;
    public int CurrentAmount;
    public bool IsSatisfied;
}

/// <summary>
/// 建造界面 ViewModel。
/// </summary>
public class BuildingViewModel
{
    // ══════════════════════════════════════════════════════
    // 事件
    // ══════════════════════════════════════════════════════

    public event Action<List<BuildingDisplayData>> OnBuildingListUpdated;
    public event Action<BuildingDisplayData> OnSelectedBuildingChanged;
    public event Action<CraftingResult, string> OnBuildResult;

    // ══════════════════════════════════════════════════════
    // 数据
    // ══════════════════════════════════════════════════════

    private readonly List<BuildingDisplayData> _buildings = new List<BuildingDisplayData>();
    private int _selectedIndex = -1;

    public int SelectedIndex => _selectedIndex;

    public BuildingDisplayData? SelectedBuilding =>
        _selectedIndex >= 0 && _selectedIndex < _buildings.Count
            ? _buildings[_selectedIndex]
            : (BuildingDisplayData?)null;

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    public void SetBuildings(List<BuildingDisplayData> buildings)
    {
        _buildings.Clear();
        _buildings.AddRange(buildings);
        _selectedIndex = _buildings.Count > 0 ? 0 : -1;

        OnBuildingListUpdated?.Invoke(_buildings);
        if (_selectedIndex >= 0)
            OnSelectedBuildingChanged?.Invoke(_buildings[_selectedIndex]);
    }

    public void SelectBuilding(int index)
    {
        if (index < 0 || index >= _buildings.Count) return;
        _selectedIndex = index;
        OnSelectedBuildingChanged?.Invoke(_buildings[_selectedIndex]);
    }

    public void NotifyBuildResult(CraftingResult result, string name)
    {
        OnBuildResult?.Invoke(result, name);
    }
}
