// 📁 Assets/_Game/04_Gameplay/Player/FSM/States/PlayerJumpState.cs
// 跳跃状态：施加跳跃力，到达顶点后转入 Fall
using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    public PlayerJumpState(PlayerController player, PlayerStateMachine fsm) : base(player, fsm) { }

    public override void OnEnter()
    {
        Player.SetAnimationState("Jump");
        Player.PerformJump();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (Player.IsDead) { FSM.ChangeState(PlayerState.Dead); return; }

        // 空中水平移动
        float moveInput = Player.MoveInput.x;
        Player.SetVelocityX(moveInput * Player.AirMoveSpeed);
        if (Mathf.Abs(moveInput) > 0.01f) Player.UpdateFacing(moveInput);

        // 到达下落阶段
        if (Player.VerticalVelocity <= 0f)
        {
            FSM.ChangeState(PlayerState.Fall);
        }
    }
}
