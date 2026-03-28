// 📁 Assets/_Game/02_Base/GameState/States/GamePlayState.cs
// 游戏进行状态
using UnityEngine;

public class GamePlayState : IState
{
    public void OnEnter()
    {
        Debug.Log("[GameState] 进入游戏");
    }

    public void OnUpdate(float deltaTime) { }
    public void OnFixedUpdate(float fixedDeltaTime) { }

    public void OnExit()
    {
        Debug.Log("[GameState] 退出游戏");
    }
}
