// 📁 02_Infrastructure/StateMachine/IState.cs
using System;
using System.Collections.Generic;

public interface IState
{
    void OnEnter();
    void OnUpdate(float deltaTime);
    void OnFixedUpdate(float fixedDeltaTime);
    void OnExit();
}

// 📁 02_Infrastructure/StateMachine/StateMachine.cs
/// <summary>
/// 通用有限状态机。玩家FSM / 敌人AI FSM / 全局游戏状态 均复用此框架。
/// </summary>
public class StateMachine<TStateKey> where TStateKey : Enum
{
    private readonly Dictionary<TStateKey, IState> _states 
        = new Dictionary<TStateKey, IState>();
    
    public IState CurrentState { get; private set; }
    public TStateKey CurrentStateKey { get; private set; }

    public event Action<TStateKey, TStateKey> OnStateChanged;  // (from, to)

    public void AddState(TStateKey key, IState state) 
        => _states[key] = state;

    public void ChangeState(TStateKey newKey)
    {
        if (!_states.TryGetValue(newKey, out var newState)) return;
        if (EqualityComparer<TStateKey>.Default.Equals(CurrentStateKey, newKey)) return;

        var prevKey = CurrentStateKey;
        CurrentState?.OnExit();
        CurrentStateKey = newKey;
        CurrentState = newState;
        CurrentState.OnEnter();
        
        OnStateChanged?.Invoke(prevKey, newKey);
    }

    public void Update(float deltaTime) => CurrentState?.OnUpdate(deltaTime);
    public void FixedUpdate(float fixedDeltaTime) => CurrentState?.OnFixedUpdate(fixedDeltaTime);
}