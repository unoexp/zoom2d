// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/06_Extensions/Mod/IModEntry.cs
// MOD 入口接口和数据提供接口。
// ══════════════════════════════════════════════════════════════════════

/// <summary>
/// MOD 入口接口。所有 MOD 的主类实现此接口。
/// </summary>
public interface IModEntry
{
    /// <summary>MOD 唯一标识</summary>
    string ModId { get; }

    /// <summary>MOD 显示名称</summary>
    string ModName { get; }

    /// <summary>MOD 版本</summary>
    string Version { get; }

    /// <summary>MOD 初始化（游戏启动时调用）</summary>
    void OnInitialize();

    /// <summary>MOD 启用</summary>
    void OnEnable();

    /// <summary>MOD 禁用</summary>
    void OnDisable();

    /// <summary>MOD 卸载</summary>
    void OnUnload();
}

/// <summary>
/// MOD 数据提供接口。允许 MOD 注入自定义数据（物品/配方/敌人等）。
/// </summary>
public interface IModDataProvider
{
    /// <summary>获取 MOD 提供的物品定义</summary>
    ItemDefinitionSO[] GetCustomItems();

    /// <summary>获取 MOD 提供的配方定义</summary>
    RecipeDefinitionSO[] GetCustomRecipes();
}
