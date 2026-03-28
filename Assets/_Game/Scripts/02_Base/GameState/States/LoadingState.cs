// 📁 Assets/_Game/02_Base/GameState/States/LoadingState.cs
// 场景加载状态
using UnityEngine;

public class LoadingState : IState
{
    public void OnEnter()
    {
        Debug.Log("[GameState] 进入加载状态");
    }

    public void OnUpdate(float deltaTime) { }
    public void OnFixedUpdate(float fixedDeltaTime) { }

    public void OnExit()
    {
        Debug.Log("[GameState] 退出加载状态");
    }
}
