// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/04_Gameplay/Map/MapChunk.cs
// 地图区块。管理一个区块内的地块数据。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 地图区块。
///
/// 核心职责：
///   · 持有固定大小的格子数据（是否已挖掘、地块类型等）
///   · 提供单格挖掘/放置接口
///   · 管理区块的加载/卸载状态
/// </summary>
public class MapChunk : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    // 常量
    // ══════════════════════════════════════════════════════

    public const int CHUNK_WIDTH = 16;
    public const int CHUNK_HEIGHT = 16;

    // ══════════════════════════════════════════════════════
    // 数据
    // ══════════════════════════════════════════════════════

    /// <summary>格子数据（0=空，>0=实心地块类型ID）</summary>
    private readonly int[,] _tiles = new int[CHUNK_WIDTH, CHUNK_HEIGHT];

    /// <summary>区块坐标（世界坐标 = ChunkCoord * CHUNK_SIZE）</summary>
    private Vector2Int _chunkCoord;

    /// <summary>所属地层ID</summary>
    private string _layerId;

    private bool _isDirty;

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    public Vector2Int ChunkCoord => _chunkCoord;
    public string LayerId => _layerId;
    public bool IsDirty => _isDirty;

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>初始化区块</summary>
    public void Initialize(Vector2Int coord, string layerId)
    {
        _chunkCoord = coord;
        _layerId = layerId;
        _isDirty = false;
    }

    /// <summary>获取格子类型</summary>
    public int GetTile(int localX, int localY)
    {
        if (localX < 0 || localX >= CHUNK_WIDTH || localY < 0 || localY >= CHUNK_HEIGHT)
            return 0;
        return _tiles[localX, localY];
    }

    /// <summary>设置格子类型</summary>
    public void SetTile(int localX, int localY, int tileType)
    {
        if (localX < 0 || localX >= CHUNK_WIDTH || localY < 0 || localY >= CHUNK_HEIGHT)
            return;
        _tiles[localX, localY] = tileType;
        _isDirty = true;
    }

    /// <summary>挖掘格子（设为空）</summary>
    public bool DigTile(int localX, int localY)
    {
        if (GetTile(localX, localY) == 0) return false; // 已空
        SetTile(localX, localY, 0);
        return true;
    }

    /// <summary>用指定类型填充整个区块</summary>
    public void Fill(int tileType)
    {
        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int y = 0; y < CHUNK_HEIGHT; y++)
            {
                _tiles[x, y] = tileType;
            }
        }
        _isDirty = true;
    }

    /// <summary>标记为已保存</summary>
    public void MarkClean()
    {
        _isDirty = false;
    }
}
