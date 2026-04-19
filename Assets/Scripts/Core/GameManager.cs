using UnityEngine;
using System;

// ============================================================
// GameManager.cs — Singleton quản lý game state machine
// ============================================================
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ---- Events ----
    /// <summary>Phát khi state thay đổi (oldState, newState)</summary>
    public static event Action<GameState, GameState> OnGameStateChanged;

    /// <summary>Phát khi điểm thay đổi</summary>
    public static event Action<int> OnScoreChanged;

    /// <summary>Phát khi combo thay đổi</summary>
    public static event Action<int> OnComboChanged;

    // ---- Inspector ----
    [Header("=== Config ===")]
    [SerializeField] private GameConfig gameConfig;
    public GameConfig Config => gameConfig;

    // ---- State ----
    private GameState _currentState = GameState.Intro;
    public GameState CurrentState => _currentState;

    private int _score;
    public int Score => _score;

    private int _combo;
    public int Combo => _combo;

    private float _musicStartTime;
    /// <summary>Thời gian (seconds) kể từ lúc nhạc bắt đầu phát</summary>
    public float MusicElapsed => Time.time - _musicStartTime;

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

    private void Start()
    {
        SetState(GameState.Intro);
    }

    // ============================================================
    // STATE MACHINE
    // ============================================================

    /// <summary>
    /// Chuyển sang trạng thái mới. Phát event OnGameStateChanged.
    /// </summary>
    public void SetState(GameState newState)
    {
        if (_currentState == newState) return;

        GameState oldState = _currentState;
        _currentState = newState;

        Debug.Log($"[GameManager] State: {oldState} → {newState}");

        // Xử lý logic riêng cho từng state
        switch (newState)
        {
            case GameState.WaitingToStart:
                // Reset score/combo khi chuẩn bị chơi
                _score = 0;
                _combo = 0;
                OnScoreChanged?.Invoke(_score);
                OnComboChanged?.Invoke(_combo);
                break;

            case GameState.Playing:
                _musicStartTime = Time.time;
                break;

            case GameState.GameOver:
                // Delay rồi chuyển CTA (sẽ xử lý bởi CTAManager)
                break;

            case GameState.CTA:
                break;
        }

        OnGameStateChanged?.Invoke(oldState, newState);
    }

    // ============================================================
    // SCORING
    // ============================================================

    /// <summary>
    /// Thêm điểm (Good Hit hoặc Perfect Hit)
    /// </summary>
    public void AddScore(int points)
    {
        _score += points;
        _combo++;
        OnScoreChanged?.Invoke(_score);
        OnComboChanged?.Invoke(_combo);
    }

    /// <summary>
    /// Reset combo (khi miss hoặc kết thúc)
    /// </summary>
    public void ResetCombo()
    {
        _combo = 0;
        OnComboChanged?.Invoke(_combo);
    }

    // ============================================================
    // CONVENIENCE
    // ============================================================

    /// <summary>
    /// Tốc độ cuộn hiện tại (tính theo score)
    /// </summary>
    public float CurrentScrollSpeed
    {
        get
        {
            if (gameConfig == null) return 4f;
            return gameConfig.GetCurrentSpeed(_score);
        }
    }

    /// <summary>
    /// Gọi khi tile bị miss
    /// </summary>
    public void TriggerGameOver()
    {
        if (_currentState != GameState.Playing) return;
        SetState(GameState.GameOver);
    }

    /// <summary>
    /// Gọi khi hoàn thành tất cả beatmap → chuyển CTA
    /// </summary>
    public void TriggerSongComplete()
    {
        if (_currentState != GameState.Playing) return;
        SetState(GameState.CTA);
    }
}
