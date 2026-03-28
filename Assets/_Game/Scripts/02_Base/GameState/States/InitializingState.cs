// 📁 Assets/_Game/02_Base/GameState/States/InitializingState.cs
// 初始化状态：加载配置、注册服务、预热对象池
using UnityEngine;

public class InitializingState : IState
{
    public void OnEnter()
    {
        Debug.Log("[GameState] 进入初始化状态");
    }

    public void OnUpdate(float deltaTime) { }
    public void OnFixedUpdate(float fixedDeltaTime) { }

    public void OnExit()
    {
        Debug.Log("[GameState] 退出初始化状态");
    }
}
