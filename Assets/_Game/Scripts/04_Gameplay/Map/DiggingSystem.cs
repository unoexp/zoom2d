// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Map/DiggingSystem.cs
// 挖掘系统。处理格子挖掘的时间、体力消耗、产出计算。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 挖掘系统。
///
/// 核心职责：
///   · 计算挖掘时间：地层硬度 ÷ 工具效率
///   · 消耗体力：每格消耗 = 地层基础消耗
///   · 验证挖掘条件（地层解锁、体力足够）
///   · 挖掘完成后：移除格子 + 生成掉落物
///   · 通过 EventBus 广播挖掘事件
///
/// 设计说明（GDD 第七章）：
///   · 挖掘时间 = 硬度系数 ÷ 工具效率系数
///   · 体力消耗 = 基础消耗 × 地层深度修正
///   · 每次下探是一次"远征"，体力限制挖掘深度
/// </summary>
public class DiggingSystem : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    [Header("默认工具效率")]
    [SerializeField] private float _defaultToolEfficiency = 1f;

    // ══════════════════════════════════════════════════════
    // 缓存引用
    // ══════════════════════════════════════════════════════

    private MapManager _mapManager;
    private SurvivalStatusSystem _survivalSystem;
    private PlayerFacade _playerFacade;

    // ══════════════════════════════════════════════════════
    // 挖掘状态
    // ══════════════════════════════════════════════════════

    private bool _isDigging;
    private float _digTimer;
    private float _digDuration;
    private Vector2Int _digTarget;
    private LayerDefinitionSO _digLayer;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    public bool IsDigging => _isDigging;
    public float DigProgress => _digDuration > 0f ? Mathf.Clamp01(_digTimer / _digDuration) : 0f;

    // ══════════════════════════════════════════════════════
    // 生命周期
    // ══════════════════════════════════════════════════════

    private void Awake()
    {
        ServiceLocator.Register<DiggingSystem>(this);
    }

    private void Start()
    {
        _mapManager = ServiceLocator.Get<MapManager>();
        _survivalSystem = ServiceLocator.Get<SurvivalStatusSystem>();

        if (ServiceLocator.TryGet<PlayerFacade>(out var player))
            _playerFacade = player;
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<DiggingSystem>();
    }

    private void Update()
    {
        if (!_isDigging) return;

        _digTimer += Time.deltaTime;
        if (_digTimer >= _digDuration)
        {
            CompleteDig();
        }
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// 尝试开始挖掘指定格子。
    /// </summary>
    /// <param name="worldPos">目标世界坐标</param>
    /// <param name="toolEfficiency">工具效率系数（默认 1.0）</param>
    /// <returns>是否成功开始挖掘</returns>
    public bool TryStartDig(Vector2 worldPos, float toolEfficiency = -1f)
    {
        if (_isDigging) return false;
        if (_mapManager == null) return false;

        // 获取格子信息
        MapManager.WorldToChunkLocal(worldPos, out var chunkCoord, out int localX, out int localY);
        var chunk = _mapManager.GetChunk(chunkCoord);
        if (chunk == null || chunk.GetTile(localX, localY) == 0)
        {
            EventBus.Publish(new DiggingFailedEvent { Reason = "该位置无可挖掘的方块" });
            return false;
        }

        // 获取地层定义
        string layerId = chunk.LayerId;
        if (string.IsNullOrEmpty(layerId))
            layerId = _mapManager.GetLayerIdAtDepth(worldPos.y);

        var layerDef = _mapManager.GetLayerDefinition(layerId);
        if (layerDef == null)
        {
            EventBus.Publish(new DiggingFailedEvent { Reason = "未知地层" });
            return false;
        }

        // 检查地层解锁
        if (!_mapManager.IsLayerUnlocked(layerId))
        {
            EventBus.Publish(new DiggingFailedEvent { Reason = $"地层 {layerDef.DisplayName} 尚未解锁" });
            return false;
        }

        // 检查体力
        float staminaCost = layerDef.StaminaCostPerTile;
        if (_playerFacade != null && !_playerFacade.HasStamina(staminaCost))
        {
            EventBus.Publish(new DiggingFailedEvent { Reason = "体力不足" });
            return false;
        }

        // 计算挖掘时间
        float efficiency = toolEfficiency > 0f ? toolEfficiency : _defaultToolEfficiency;
        _digDuration = layerDef.Hardness / efficiency;
        _digTimer = 0f;
        _isDigging = true;
        _digTarget = new Vector2Int(
            chunkCoord.x * MapChunk.CHUNK_WIDTH + localX,
            chunkCoord.y * MapChunk.CHUNK_HEIGHT + localY
        );
        _digLayer = layerDef;

        // 消耗体力
        if (_playerFacade != null)
            _playerFacade.ConsumeStamina(staminaCost);

        EventBus.Publish(new DiggingStartedEvent
        {
            TilePosition = _digTarget,
            LayerId = layerId,
            Duration = _digDuration
        });

        return true;
    }

    /// <summary>取消当前挖掘</summary>
    public void CancelDig()
    {
        _isDigging = false;
        _digTimer = 0f;
    }

    // ══════════════════════════════════════════════════════
    // 内部方法
    // ══════════════════════════════════════════════════════

    /// <summary>挖掘完成处理</summary>
    private void CompleteDig()
    {
        _isDigging = false;

        // 移除格子
        Vector2 worldPos = new Vector2(_digTarget.x + 0.5f, _digTarget.y + 0.5f);
        MapManager.WorldToChunkLocal(worldPos, out var chunkCoord, out int localX, out int localY);
        var chunk = _mapManager.GetChunk(chunkCoord);
        if (chunk != null)
        {
            chunk.DigTile(localX, localY);
        }

        // 生成掉落物
        if (_digLayer != null && _digLayer.Drops != null)
        {
            for (int i = 0; i < _digLayer.Drops.Length; i++)
            {
                var drop = _digLayer.Drops[i];
                if (drop.Item == null) continue;
                if (Random.value > drop.DropChance) continue;

                int amount = Random.Range(drop.MinAmount, drop.MaxAmount + 1);
                if (amount <= 0) continue;

                // 通过事件通知生成掉落物
                EventBus.Publish(new ItemAddedToInventoryEvent
                {
                    ItemId = drop.Item.ItemId,
                    Amount = amount,
                    SlotIndex = -1,
                    ContainerId = ""
                });
            }
        }

        EventBus.Publish(new DiggingCompletedEvent
        {
            TilePosition = _digTarget,
            LayerId = _digLayer != null ? _digLayer.LayerId : ""
        });
    }
}
