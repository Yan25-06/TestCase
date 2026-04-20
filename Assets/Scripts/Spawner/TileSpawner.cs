using UnityEngine;

// ============================================================
// TileSpawner.cs — Spawn tile theo beatmap[]
// Duyệt qua GameConfig.beatmap, spawn tile đúng timestamp
// ============================================================
public class TileSpawner : MonoBehaviour
{
    public static TileSpawner Instance { get; private set; }

    [Header("=== References ===")]
    [SerializeField] private GameConfig gameConfig;

    // ---- Runtime ----
    private int _nextBeatIndex = 0;
    private bool _isSpawning = false;
    private bool _startTileSpawned = false;

    /// <summary>
    /// Thời gian cần spawn trước so với timestamp beat,
    /// tính bằng khoảng cách / tốc độ (tile cần thời gian rơi từ spawnY đến hitWindowTop)
    /// </summary>
    private float _spawnLeadTime;

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
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        CalculateSpawnLeadTime();
    }

    private void Update()
    {
        if (!_isSpawning) return;
        if (GameManager.Instance == null) return;
        if (gameConfig == null || gameConfig.beatmap == null) return;

        float musicTime = GameManager.Instance.MusicElapsed;

        // Spawn tiles trước thời điểm beat đủ leadTime
        while (_nextBeatIndex < gameConfig.beatmap.Length)
        {
            BeatmapEntry beat = gameConfig.beatmap[_nextBeatIndex];
            float spawnTime = beat.time - _spawnLeadTime;

            if (musicTime >= spawnTime)
            {
                SpawnTile(beat);
                _nextBeatIndex++;
            }
            else
            {
                break; // Chưa đến lúc spawn beat tiếp theo
            }
        }

        // Kiểm tra đã spawn hết beatmap chưa
        if (_nextBeatIndex >= gameConfig.beatmap.Length)
        {
            _isSpawning = false;
            // Đợi tất cả tiles active xử lý xong rồi kết thúc
            // (sẽ check trong LateUpdate hoặc sau khi tile cuối completed)
        }
    }

    private void LateUpdate()
    {
        // Sau khi hết beatmap, check nếu tất cả tile đã xong → hoàn thành bài
        if (!_isSpawning &&
            _nextBeatIndex > 0 &&
            _nextBeatIndex >= (gameConfig != null && gameConfig.beatmap != null ? gameConfig.beatmap.Length : 0) &&
            GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameState.Playing)
        {
            if (TilePool.Instance != null && TilePool.Instance.ActiveTiles.Count == 0)
            {
                GameManager.Instance.TriggerSongComplete();
            }
        }
    }

    // ============================================================
    // STATE HANDLING
    // ============================================================
    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Intro:
                SpawnStartTile(); // Start Tile xuất hiện ngay từ Intro
                break;

            case GameState.Playing:
                StartSpawning();
                break;

            case GameState.GameOver:
            case GameState.CTA:
                StopSpawning();
                break;
        }
    }

    // ============================================================
    // SPAWN LOGIC
    // ============================================================

    /// <summary>
    /// Spawn Start Tile ở giữa màn hình
    /// </summary>
    private void SpawnStartTile()
    {
        if (_startTileSpawned) return;
        if (TilePool.Instance == null) return;

        TileController startTile = TilePool.Instance.Get(TileType.Start);
        if (startTile != null)
        {
            // Start tile đặt tại lane thứ 3 (index 2, 0-based)
            const int startLane = 2;

            float laneX = OrientationManager.Instance != null
                ? OrientationManager.Instance.GetLaneX(startLane)
                : (gameConfig != null ? gameConfig.fallbackLaneXPositions[startLane] : 0f);

            float centerY = gameConfig != null ? (gameConfig.hitWindowTop + 2f) : 0f;

            startTile.transform.position = new Vector3(laneX, centerY, 0f);

            // Scale tile vừa khít 1 lane (giống cách Init() làm cho tile thường)
            if (OrientationManager.Instance != null)
            {
                float laneWidth   = OrientationManager.Instance.GetLaneWidth();
                float padding     = gameConfig != null ? gameConfig.horizontalPadding : 0f;
                float targetWidth = laneWidth - (padding * 2f);

                // Tìm sprite đại diện (body ưu tiên, fallback sang head)
                SpriteRenderer sr = startTile.bodyRenderer != null ? startTile.bodyRenderer
                                  : startTile.headRenderer;

                if (sr != null && sr.sprite != null)
                {
                    float spriteWidth = sr.sprite.bounds.size.x;
                    if (spriteWidth > 0f)
                    {
                        float scaleX = targetWidth / spriteWidth;
                        startTile.transform.localScale = new Vector3(scaleX, scaleX, 1f);
                    }
                }
            }

            startTile.laneIndex = startLane;
            startTile.tileState = TileState.InHitZone; // Sẵn sàng để tap
            startTile.gameObject.SetActive(true);

            _startTileSpawned = true;
            Debug.Log($"[TileSpawner] Start Tile spawned at lane {startLane} (x={laneX:F2})");
        }
    }

    /// <summary>
    /// Bắt đầu spawn tiles theo beatmap
    /// </summary>
    private void StartSpawning()
    {
        _nextBeatIndex = 0;
        _isSpawning = true;
        CalculateSpawnLeadTime();

        Debug.Log($"[TileSpawner] Starting — {gameConfig.beatmap.Length} beats to spawn");
    }

    /// <summary>
    /// Dừng spawn
    /// </summary>
    private void StopSpawning()
    {
        _isSpawning = false;
    }

    /// <summary>
    /// Spawn 1 tile theo BeatmapEntry
    /// </summary>
    private void SpawnTile(BeatmapEntry beat)
    {
        if (TilePool.Instance == null) return;

        TileController tile = TilePool.Instance.Get(beat.type);
        if (tile == null)
        {
            Debug.LogError($"[TileSpawner] Không lấy được tile type={beat.type} từ pool!");
            return;
        }

        float speed = GameManager.Instance != null
            ? GameManager.Instance.CurrentScrollSpeed
            : (gameConfig != null ? gameConfig.baseScrollSpeed : 4f);

        tile.Init(beat.lane, speed, gameConfig);

        Debug.Log($"[TileSpawner] Spawned {beat.type} on lane {beat.lane} at beat time {beat.time:F2}s");
    }

    // ============================================================
    // HELPERS
    // ============================================================

    /// <summary>
    /// Tính thời gian tile cần rơi từ spawnY đến hitWindowTop
    /// để spawn trước đúng lúc
    /// </summary>
    private void CalculateSpawnLeadTime()
    {
        if (gameConfig == null) { _spawnLeadTime = 3f; return; }

        float distance = gameConfig.spawnY - gameConfig.hitWindowTop;
        float speed = gameConfig.baseScrollSpeed;

        _spawnLeadTime = distance / speed;
        Debug.Log($"[TileSpawner] Lead time = {_spawnLeadTime:F2}s (distance={distance}, speed={speed})");
    }

    /// <summary>
    /// Reset spawner (khi retry game)
    /// </summary>
    public void ResetSpawner()
    {
        _nextBeatIndex = 0;
        _isSpawning = false;
        _startTileSpawned = false;

        if (TilePool.Instance != null)
            TilePool.Instance.ReturnAll();
    }
}
