// 📁 02_Infrastructure/Singleton/MonoSingleton.cs
using UnityEngine;


/// <summary>
/// MonoBehaviour 单例基类。
/// 仅用于基础设施层管理器（AudioManager/VFXManager等），
/// 业务逻辑系统优先使用 ServiceLocator。
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                    Debug.LogError($"[Singleton] {typeof(T).Name} 实例不存在！请检查场景中是否存在该对象。");
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = (T)this;
        DontDestroyOnLoad(gameObject);
        OnInitialize();
    }

    /// <summary>替代Awake的初始化入口，子类重写此方法</summary>
    protected virtual void OnInitialize() { }
    
    protected virtual void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}