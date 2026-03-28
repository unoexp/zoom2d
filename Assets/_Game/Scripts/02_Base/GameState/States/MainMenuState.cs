// 📁 Assets/_Game/02_Base/GameState/States/MainMenuState.cs
// 主菜单状态
using UnityEngine;

public class MainMenuState : IState
{
    public void OnEnter()
    {
        Debug.Log("[GameState] 进入主菜单");
    }

    public void OnUpdate(float deltaTime) { }
    public void OnFixedUpdate(float fixedDeltaTime) { }

    public void OnExit()
    {
        Debug.Log("[GameState] 退出主菜单");
    }
}
