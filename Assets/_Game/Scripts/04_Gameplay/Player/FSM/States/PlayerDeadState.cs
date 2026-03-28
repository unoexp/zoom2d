// 📁 Assets/_Game/04_Gameplay/Player/FSM/States/PlayerDeadState.cs
// 死亡状态：禁用所有输入，播放死亡动画
using UnityEngine;

public class PlayerDeadState : PlayerStateBase
{
    public PlayerDeadState(PlayerController player, PlayerStateMachine fsm) : base(player, fsm) { }

    public override void OnEnter()
    {
        Player.SetAnimationState("Dead");
        Player.SetVelocityX(0f);
        Player.DisableInput();
        Debug.Log("[Player] 玩家死亡");
    }

    public override void OnUpdate(float deltaTime)
    {
        // 死亡状态不做任何状态转移，等待外部系统（GameStateManager）处理
    }

    public override void OnExit()
    {
        Player.EnableInput();
    }
}
