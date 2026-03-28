// 📁 Assets/_Game/04_Gameplay/Player/FSM/States/PlayerFallState.cs
// 下落状态：空中下落，着地后转回 Idle
using UnityEngine;

public class PlayerFallState : PlayerStateBase
{
    public PlayerFallState(PlayerController player, PlayerStateMachine fsm) : base(player, fsm) { }

    public override void OnEnter()
    {
        Player.SetAnimationState("Fall");
    }

    public override void OnUpdate(float deltaTime)
    {
        if (Player.IsDead) { FSM.ChangeState(PlayerState.Dead); return; }

        // 空中水平移动
        float moveInput = Player.MoveInput.x;
        Player.SetVelocityX(moveInput * Player.AirMoveSpeed);
        if (Mathf.Abs(moveInput) > 0.01f) Player.UpdateFacing(moveInput);

        // 落地检测
        if (Player.IsGrounded)
        {
            if (Mathf.Abs(Player.MoveInput.x) > 0.01f)
                FSM.ChangeState(Player.IsRunning ? PlayerState.Run : PlayerState.Walk);
            else
                FSM.ChangeState(PlayerState.Idle);
        }
    }
}
