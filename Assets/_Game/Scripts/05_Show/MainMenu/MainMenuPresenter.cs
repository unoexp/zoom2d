// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/05_Show/MainMenu/MainMenuPresenter.cs
// 主菜单Presenter。处理菜单交互逻辑。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单 Presenter。
///
/// 核心职责：
///   · 初始化主菜单状态（检查存档是否存在）
///   · 处理按钮交互 → 调用 GameStateManager / SaveLoadSystem
///   · 管理场景切换逻辑
/// </summary>
public class MainMenuPresenter : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 引用
    // ══════════════════════════════════════════════════════

    [SerializeField] private MainMenuView _menuView;

    [Header("场景配置")]
    [Tooltip("游戏主场景名称")]
    [SerializeField] private string _gameSceneName = "GameScene";

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Start()
    {
        if (_menuView != null)
        {
            // 绑定按钮事件
            _menuView.OnNewGameClicked += HandleNewGame;
            _menuView.OnContinueClicked += HandleContinue;
            _menuView.OnSettingsClicked += HandleSettings;
            _menuView.OnQuitClicked += HandleQuit;

            // 注册到 UIManager 并显示
            var uiManager = UIManager.Instance;
            if (uiManager != null)
            {
                uiManager.RegisterPanel(_menuView);
            }

            // 初始化菜单状态
            InitializeMenu();

            _menuView.Show();
        }
    }

    private void OnDestroy()
    {
        if (_menuView != null)
        {
            _menuView.OnNewGameClicked -= HandleNewGame;
            _menuView.OnContinueClicked -= HandleContinue;
            _menuView.OnSettingsClicked -= HandleSettings;
            _menuView.OnQuitClicked -= HandleQuit;
        }
    }

    // ══════════════════════════════════════════════════════
    // 初始化
    // ══════════════════════════════════════════════════════

    private void InitializeMenu()
    {
        // 检查是否有存档
        bool hasSave = false;
        if (ServiceLocator.TryGet<SaveLoadSystem>(out var saveSystem))
        {
            hasSave = saveSystem.HasSaveData();
        }

        _menuView.SetContinueAvailable(hasSave);
    }

    // ══════════════════════════════════════════════════════
    // 按钮处理
    // ══════════════════════════════════════════════════════

    private void HandleNewGame()
    {
        Debug.Log("[MainMenu] 新游戏");

        // 通知状态机切换到加载状态
        EventBus.Publish(new GameStateChangedEvent
        {
            PreviousState = GameState.MainMenu,
            NewState = GameState.Loading
        });

        SceneManager.LoadScene(_gameSceneName);
    }

    private void HandleContinue()
    {
        Debug.Log("[MainMenu] 继续游戏");

        // 加载场景后由 SaveLoadSystem 恢复存档
        EventBus.Publish(new GameStateChangedEvent
        {
            PreviousState = GameState.MainMenu,
            NewState = GameState.Loading
        });

        SceneManager.LoadScene(_gameSceneName);
    }

    private void HandleSettings()
    {
        Debug.Log("[MainMenu] 打开设置");
        // 后续实现设置面板时，通过 UIManager.OpenPanel("SettingsPanel") 打开
    }

    private void HandleQuit()
    {
        Debug.Log("[MainMenu] 退出游戏");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
