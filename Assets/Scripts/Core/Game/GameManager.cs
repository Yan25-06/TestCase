using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private PoolManager poolManager;

    [Header("Runtime")]
    [SerializeField] private GameState currentState = GameState.Idle;
    [SerializeField] private int currentScore;
    [SerializeField] private float currentScrollSpeed;

    public event Action<GameState> OnGameStateChanged;
    public event Action<int> OnScoreChanged;

    public GameConfig Config => gameConfig;
    public PoolManager Pool => poolManager;
    public GameState CurrentState => currentState;
    public int CurrentScore => currentScore;
    public float CurrentScrollSpeed => currentScrollSpeed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ApplyInitialRuntimeValues();
    }

    public void StartGame()
    {
        if (currentState == GameState.Playing)
        {
            return;
        }

        ResetRuntimeValues();

        if (poolManager != null)
        {
            poolManager.PrewarmAll();
        }

        SetState(GameState.Playing);
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused)
        {
            return;
        }

        SetState(GameState.Playing);
    }

    public void AddScore(int amount)
    {
        if (currentState != GameState.Playing || amount <= 0)
        {
            return;
        }

        currentScore += amount;
        currentScrollSpeed = CalculateScrollSpeed(currentScore);
        OnScoreChanged?.Invoke(currentScore);
    }

    public void TriggerGameOver()
    {
        if (currentState == GameState.GameOver)
        {
            return;
        }

        SetState(GameState.GameOver);

        if (poolManager != null)
        {
            poolManager.DespawnAllActive();
        }
    }

    public void ResetToIdle()
    {
        ResetRuntimeValues();
        SetState(GameState.Idle);
    }

    private void ApplyInitialRuntimeValues()
    {
        currentScore = 0;
        currentScrollSpeed = gameConfig != null ? gameConfig.BaseScrollSpeed : 0f;
    }

    private void ResetRuntimeValues()
    {
        currentScore = 0;
        currentScrollSpeed = gameConfig != null ? gameConfig.BaseScrollSpeed : 0f;
        OnScoreChanged?.Invoke(currentScore);
    }

    private float CalculateScrollSpeed(int score)
    {
        if (gameConfig == null)
        {
            return currentScrollSpeed;
        }

        return gameConfig.BaseScrollSpeed + score * gameConfig.SpeedIncreasePerScore;
    }

    private void SetState(GameState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
    }
}
