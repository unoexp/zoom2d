// 📁 Assets/_Game/02_Base/GameState/States/GameOverState.cs
// 游戏结束状态
using UnityEngine;

public class GameOverState : IState
{
    public void OnEnter()
    {
        Debug.Log("[GameState] 游戏结束");
        Time.timeScale = 0f;
    }

    public void OnUpdate(float deltaTime) { }
    public void OnFixedUpdate(float fixedDeltaTime) { }

    public void OnExit()
    {
        Time.timeScale = 1f;
        Debug.Log("[GameState] 退出游戏结束状态");
    }
}
