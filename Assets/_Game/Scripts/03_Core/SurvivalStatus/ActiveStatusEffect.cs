// ─────────────────────────────────────────────────────────────────────
// 📁 Assets/_Game/03_CoreSystems/SurvivalStatus/ActiveStatusEffect.cs
// 运行时激活的状态效果包装器，持有剩余时间
// ─────────────────────────────────────────────────────────────────────
public class ActiveStatusEffect
{
    public IStatusEffect Effect        { get; }
    public float         RemainingTime { get; private set; }

    public ActiveStatusEffect(IStatusEffect effect)
    {
        Effect        = effect;
        RemainingTime = effect.Duration; // -1 = 永久
    }

    public void ReduceTimer(float dt)
    {
        if (RemainingTime < 0f) return; // 永久效果不计时
        RemainingTime -= dt;
    }

    /// <summary>刷新计时器（非叠加效果重复施加时调用）</summary>
    public void ResetTimer()
        => RemainingTime = Effect.Duration;
}
