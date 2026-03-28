// 📁 Assets/_Game/02_Base/GameState/States/PauseState.cs
// 暂停状态：冻结游戏时间
using UnityEngine;

public class PauseState : IState
{
    public void OnEnter()
    {
        Debug.Log("[GameState] 游戏暂停");
        Time.timeScale = 0f;
    }

    public void OnUpdate(float deltaTime) { }
    public void OnFixedUpdate(float fixedDeltaTime) { }

    public void OnExit()
    {
        Time.timeScale = 1f;
        Debug.Log("[GameState] 恢复游戏");
    }
}
