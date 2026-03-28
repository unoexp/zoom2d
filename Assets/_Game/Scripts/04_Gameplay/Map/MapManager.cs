// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Map/MapManager.cs
// 地图管理器。管理区块的加载/卸载和世界坐标转换。
// ══════════════════════════════════════════════════════════════════════
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图管理器。
///
/// 核心职责：
///   · 管理所有已加载的 MapChunk
///   · 根据玩家位置动态加载/卸载区块
///   · 提供世界坐标 ↔ 区块坐标 ↔ 本地格子坐标转换
///   · 管理地层解锁状态
/// </summary>
public class MapManager : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("地层数据")]
    [SerializeField] private LayerDefinitionSO[] _layerDefinitions;

    [Header("区块加载")]
    [Tooltip("玩家周围加载的区块半径")]
    [SerializeField] private int _loadRadius = 2;

    [Tooltip("区块卸载距离（格子）")]
    [SerializeField] private int _unloadRadius = 4;

    [Header("区块预制体")]
    [SerializeField] private GameObject _chunkPrefab;

    // ══════════════════════════════════════════════════════
    // 数据
    // ══════════════════════════════════════════════════════

    /// <summary>已加载的区块（坐标 → 区块）</summary>
    private readonly Dictionary<Vector2Int, MapChunk> _loadedChunks
        = new Dictionary<Vector2Int, MapChunk>();

    /// <summary>LayerId → 地层定义</summary>
    private readonly Dictionary<string, LayerDefinitionSO> _layerMap
        = new Dictionary<string, LayerDefinitionSO>();

    /// <summary>已解锁的地层ID</summary>
    private readonly HashSet<string> _unlockedLayers = new HashSet<string>();

    private Transform _playerTransform;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        ServiceLocator.Register<MapManager>(this);

        if (_layerDefinitions != null)
        {
            for (int i = 0; i < _layerDefinitions.Length; i++)
            {
                var layer = _layerDefinitions[i];
                if (layer == null || string.IsNullOrEmpty(layer.LayerId)) continue;
                _layerMap[layer.LayerId] = layer;
                if (layer.UnlockedByDefault)
                    _unlockedLayers.Add(layer.LayerId);
            }
        }
    }

    private void Start()
    {
        // 获取玩家 Transform
        if (ServiceLocator.TryGet<PlayerFacade>(out var player))
            _playerTransform = player.transform;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<BuildCompletedEvent>(OnBuildCompleted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BuildCompletedEvent>(OnBuildCompleted);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<MapManager>();
    }

    private void LateUpdate()
    {
        if (_playerTransform != null)
        {
            UpdateChunkLoading();
        }
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>获取地层定义</summary>
    public LayerDefinitionSO GetLayerDefinition(string layerId)
    {
        _layerMap.TryGetValue(layerId, out var def);
        return def;
    }

    /// <summary>检查地层是否已解锁</summary>
    public bool IsLayerUnlocked(string layerId)
    {
        return _unlockedLayers.Contains(layerId);
    }

    /// <summary>解锁地层</summary>
    public void UnlockLayer(string layerId)
    {
        _unlockedLayers.Add(layerId);
    }

    /// <summary>获取指定世界坐标的格子类型</summary>
    public int GetTileAt(Vector2 worldPos)
    {
        WorldToChunkLocal(worldPos, out var chunkCoord, out int localX, out int localY);
        if (_loadedChunks.TryGetValue(chunkCoord, out var chunk))
            return chunk.GetTile(localX, localY);
        return -1; // 未加载
    }

    /// <summary>获取指定区块</summary>
    public MapChunk GetChunk(Vector2Int chunkCoord)
    {
        _loadedChunks.TryGetValue(chunkCoord, out var chunk);
        return chunk;
    }

    /// <summary>根据深度获取对应地层ID</summary>
    public string GetLayerIdAtDepth(float depth)
    {
        if (_layerDefinitions == null) return null;

        LayerDefinitionSO best = null;
        for (int i = 0; i < _layerDefinitions.Length; i++)
        {
            var layer = _layerDefinitions[i];
            if (layer == null) continue;
            if (depth <= layer.Depth && (best == null || layer.Depth > best.Depth))
                best = layer;
        }
        return best != null ? best.LayerId : null;
    }

    // ══════════════════════════════════════════════════════
    // 坐标转换
    // ══════════════════════════════════════════════════════

    /// <summary>世界坐标 → 区块坐标 + 本地坐标</summary>
    public static void WorldToChunkLocal(Vector2 worldPos, out Vector2Int chunkCoord,
        out int localX, out int localY)
    {
        int wx = Mathf.FloorToInt(worldPos.x);
        int wy = Mathf.FloorToInt(worldPos.y);

        chunkCoord = new Vector2Int(
            Mathf.FloorToInt((float)wx / MapChunk.CHUNK_WIDTH),
            Mathf.FloorToInt((float)wy / MapChunk.CHUNK_HEIGHT)
        );

        localX = ((wx % MapChunk.CHUNK_WIDTH) + MapChunk.CHUNK_WIDTH) % MapChunk.CHUNK_WIDTH;
        localY = ((wy % MapChunk.CHUNK_HEIGHT) + MapChunk.CHUNK_HEIGHT) % MapChunk.CHUNK_HEIGHT;
    }

    // ══════════════════════════════════════════════════════
    // 区块加载/卸载
    // ══════════════════════════════════════════════════════

    private void UpdateChunkLoading()
    {
        Vector2 playerPos = _playerTransform.position;
        WorldToChunkLocal(playerPos, out var centerChunk, out _, out _);

        // 加载范围内的区块
        for (int x = -_loadRadius; x <= _loadRadius; x++)
        {
            for (int y = -_loadRadius; y <= _loadRadius; y++)
            {
                var coord = new Vector2Int(centerChunk.x + x, centerChunk.y + y);
                if (!_loadedChunks.ContainsKey(coord))
                {
                    LoadChunk(coord);
                }
            }
        }

        // 卸载超出范围的区块
        var toUnload = new List<Vector2Int>();
        foreach (var kvp in _loadedChunks)
        {
            int dx = Mathf.Abs(kvp.Key.x - centerChunk.x);
            int dy = Mathf.Abs(kvp.Key.y - centerChunk.y);
            if (dx > _unloadRadius || dy > _unloadRadius)
                toUnload.Add(kvp.Key);
        }

        for (int i = 0; i < toUnload.Count; i++)
        {
            UnloadChunk(toUnload[i]);
        }
    }

    private void LoadChunk(Vector2Int coord)
    {
        if (_chunkPrefab == null) return;

        var go = Instantiate(_chunkPrefab, transform);
        go.name = $"Chunk_{coord.x}_{coord.y}";
        go.transform.position = new Vector3(
            coord.x * MapChunk.CHUNK_WIDTH,
            coord.y * MapChunk.CHUNK_HEIGHT, 0f);

        var chunk = go.GetComponent<MapChunk>();
        if (chunk == null) chunk = go.AddComponent<MapChunk>();

        string layerId = GetLayerIdAtDepth(coord.y * MapChunk.CHUNK_HEIGHT);
        chunk.Initialize(coord, layerId);

        // 地下区块填充实心，地表以上为空
        if (coord.y < 0)
            chunk.Fill(1);

        _loadedChunks[coord] = chunk;
    }

    private void UnloadChunk(Vector2Int coord)
    {
        if (_loadedChunks.TryGetValue(coord, out var chunk))
        {
            _loadedChunks.Remove(coord);
            if (chunk != null)
                Destroy(chunk.gameObject);
        }
    }

    // ══════════════════════════════════════════════════════
    // 事件处理
    // ══════════════════════════════════════════════════════

    private void OnBuildCompleted(BuildCompletedEvent evt)
    {
        // 建造完成后检查是否解锁新地层
        if (ServiceLocator.TryGet<BuildingSystem>(out var buildingSystem))
        {
            var def = buildingSystem.GetDefinition(evt.BuildingId);
            if (def != null && !string.IsNullOrEmpty(def.UnlocksLayerId))
            {
                UnlockLayer(def.UnlocksLayerId);
                Debug.Log($"[MapManager] 解锁地层: {def.UnlocksLayerId}");
            }
        }
    }
}
