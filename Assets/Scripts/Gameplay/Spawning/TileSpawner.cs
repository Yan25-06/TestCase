using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    [Header("Spawner")]
    [SerializeField] private string tilePoolKey = "tile";
    [SerializeField] private TileType spawnTileType = TileType.ShortTile;
    [SerializeField] private Transform spawnedTileParent;
    [SerializeField] private int maxConsecutiveSameLane = 2;
    [SerializeField] private int randomSeed = -1;
    [SerializeField] private bool autoStartWhenGamePlaying = true;

    private SpawnPatternProvider patternProvider;
    private readonly List<SpawnSequence> spawnHistory = new List<SpawnSequence>();

    private int spawnIndex;
    private float elapsedTime;
    private float nextSpawnAt;
    private bool isSpawning;
    private bool isBoundToGameManager;

    public IReadOnlyList<SpawnSequence> SpawnHistory => spawnHistory;

    private void Awake()
    {
        RebuildPatternProvider();
    }

    private void OnEnable()
    {
        TryBindToGameManager();
    }

    private void OnDisable()
    {
        if (isBoundToGameManager && GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        isBoundToGameManager = false;
    }

    private void Update()
    {
        if (!isBoundToGameManager)
        {
            TryBindToGameManager();
        }

        if (!isSpawning)
        {
            return;
        }

        GameManager manager = GameManager.Instance;
        if (manager == null || manager.Config == null || manager.Pool == null)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        while (elapsedTime >= nextSpawnAt)
        {
            SpawnSingleTile(manager);
            nextSpawnAt += manager.Config.SpawnInterval;
        }
    }

    public void BeginSpawning()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null || manager.Config == null || manager.Pool == null)
        {
            isSpawning = false;
            return;
        }

        RebuildPatternProvider();
        patternProvider.Reset();

        spawnHistory.Clear();
        spawnIndex = 0;
        elapsedTime = 0f;
        nextSpawnAt = 0f;
        isSpawning = true;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (!autoStartWhenGamePlaying)
        {
            return;
        }

        if (state == GameState.Playing)
        {
            BeginSpawning();
        }
        else
        {
            StopSpawning();
        }
    }

    private void SpawnSingleTile(GameManager manager)
    {
        int laneIndex = patternProvider.GetNextLane();
        if (!manager.Config.TryGetLaneX(laneIndex, out float laneX))
        {
            return;
        }

        Vector3 spawnPos = new Vector3(laneX, manager.Config.SpawnY, 0f);
        GameObject instance = manager.Pool.Spawn(tilePoolKey, spawnPos, Quaternion.identity, spawnedTileParent);
        if (instance == null)
        {
            return;
        }

        TileController tile = instance.GetComponent<TileController>();
        if (tile != null)
        {
            tile.Configure(laneIndex, tilePoolKey, spawnTileType);
        }

        spawnHistory.Add(new SpawnSequence(spawnIndex, laneIndex, elapsedTime));
        spawnIndex++;
    }

    private void RebuildPatternProvider()
    {
        int laneCount = 4;
        if (GameManager.Instance != null && GameManager.Instance.Config != null)
        {
            laneCount = Mathf.Max(1, GameManager.Instance.Config.LaneCount);
        }

        patternProvider = new SpawnPatternProvider(laneCount, maxConsecutiveSameLane, randomSeed);
    }

    private void TryBindToGameManager()
    {
        if (isBoundToGameManager || GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        isBoundToGameManager = true;

        if (autoStartWhenGamePlaying && GameManager.Instance.CurrentState == GameState.Playing)
        {
            BeginSpawning();
        }
    }
}
