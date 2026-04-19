using UnityEngine;
using System.Collections.Generic;

// ============================================================
// TilePool.cs — Object Pooling cho Tile
// Tối ưu WebGL: tránh Instantiate/Destroy runtime
// ============================================================
public class TilePool : MonoBehaviour
{
    public static TilePool Instance { get; private set; }

    [Header("=== Prefabs ===")]
    [Tooltip("Prefab Short Tile")]
    [SerializeField] private GameObject shortTilePrefab;

    [Tooltip("Prefab Long Tile")]
    [SerializeField] private GameObject longTilePrefab;

    [Tooltip("Prefab Start Tile")]
    [SerializeField] private GameObject startTilePrefab;

    [Header("=== Pool Config ===")]
    [Tooltip("Số lượng pre-spawn cho mỗi loại")]
    [SerializeField] private int initialPoolSize = 5;

    // ---- Pools ----
    private Queue<TileController> _shortPool = new Queue<TileController>();
    private Queue<TileController> _longPool  = new Queue<TileController>();
    private Queue<TileController> _startPool = new Queue<TileController>();

    // Tracking tất cả active tiles
    private List<TileController> _activeTiles = new List<TileController>();
    public IReadOnlyList<TileController> ActiveTiles => _activeTiles;

    // ============================================================
    // LIFECYCLE
    // ============================================================
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        PrewarmPools();
    }

    /// <summary>
    /// Pre-spawn tiles vào pool trước để tránh lag runtime
    /// </summary>
    private void PrewarmPools()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            if (shortTilePrefab != null) _shortPool.Enqueue(CreateTile(shortTilePrefab));
            if (longTilePrefab  != null) _longPool.Enqueue(CreateTile(longTilePrefab));
        }

        // Start tile thường chỉ cần 1
        if (startTilePrefab != null) _startPool.Enqueue(CreateTile(startTilePrefab));
    }

    private TileController CreateTile(GameObject prefab)
    {
        GameObject go = Instantiate(prefab, transform);
        go.SetActive(false);

        TileController tc = go.GetComponent<TileController>();
        if (tc == null)
        {
            Debug.LogError($"[TilePool] Prefab {prefab.name} thiếu TileController!");
        }

        return tc;
    }

    // ============================================================
    // GET FROM POOL
    // ============================================================

    /// <summary>
    /// Lấy tile từ pool theo loại. Tạo mới nếu pool hết.
    /// </summary>
    public TileController Get(TileType type)
    {
        TileController tile = null;

        switch (type)
        {
            case TileType.Short:
                tile = GetFromQueue(_shortPool, shortTilePrefab);
                break;
            case TileType.Long:
                tile = GetFromQueue(_longPool, longTilePrefab);
                break;
            case TileType.Start:
                tile = GetFromQueue(_startPool, startTilePrefab);
                break;
        }

        if (tile != null)
        {
            _activeTiles.Add(tile);
        }

        return tile;
    }

    private TileController GetFromQueue(Queue<TileController> pool, GameObject prefab)
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        // Pool hết → tạo mới
        if (prefab != null)
            return CreateTile(prefab);

        Debug.LogError("[TilePool] Không có prefab để tạo tile mới!");
        return null;
    }

    // ============================================================
    // RETURN TO POOL
    // ============================================================

    /// <summary>
    /// Trả tile về pool sau khi dùng xong
    /// </summary>
    public void Return(TileController tile)
    {
        if (tile == null) return;

        _activeTiles.Remove(tile);
        tile.ResetTile();

        switch (tile.tileType)
        {
            case TileType.Short:
                _shortPool.Enqueue(tile);
                break;
            case TileType.Long:
                _longPool.Enqueue(tile);
                break;
            case TileType.Start:
                _startPool.Enqueue(tile);
                break;
        }
    }

    // ============================================================
    // UTILITY
    // ============================================================

    /// <summary>
    /// Trả tất cả active tiles về pool (khi reset game)
    /// </summary>
    public void ReturnAll()
    {
        // Tạo copy để tránh modify collection during iteration
        var tiles = new List<TileController>(_activeTiles);
        foreach (var tile in tiles)
        {
            Return(tile);
        }
    }

    /// <summary>
    /// Lấy tile đang active trong Hit Zone trên lane nhất định
    /// </summary>
    public TileController GetTileInHitZone(int lane)
    {
        foreach (var tile in _activeTiles)
        {
            if (tile.laneIndex == lane &&
                (tile.tileState == TileState.InHitZone || tile.tileState == TileState.Holding))
            {
                return tile;
            }
        }
        return null;
    }

    /// <summary>
    /// Lấy tile scrolling thấp nhất (gần hit zone nhất) trên lane
    /// </summary>
    public TileController GetLowestScrollingTile(int lane)
    {
        TileController lowest = null;
        float lowestY = float.MaxValue;

        foreach (var tile in _activeTiles)
        {
            if (tile.laneIndex == lane && tile.tileState == TileState.Scrolling)
            {
                float y = tile.transform.position.y;
                if (y < lowestY)
                {
                    lowestY = y;
                    lowest = tile;
                }
            }
        }
        return lowest;
    }
}
