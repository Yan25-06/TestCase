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
    private bool _initialized = false;
    private GameState _currentState = GameState.Intro;
    public GameState CurrentState => _currentState;

    private int _score;
    public int Score => _score;

    private int _combo;
    public int Combo => _combo;

    private float _musicStartTime;
    private bool _musicScheduled = false;

    /// <summary>
    /// Thời gian (seconds) kể từ lúc game bắt đầu Playing.
    /// Âm trong khoảng musicStartDelay đầu tiên (countdown),
    /// rồi tăng dần song song với nhạc.
    /// </summary>
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
        // Force log + event cho lần đầu tiên
        _initialized = false;
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
        // Cho phép lần đầu tiên chạy dù state giống nhau
        if (_initialized && _currentState == newState) return;

        GameState oldState = _currentState;
        _currentState = newState;
        _initialized = true;

#if UNITY_EDITOR
        Debug.Log($"[GameManager] State: {oldState} → {newState}");
#endif

        // Xử lý logic riêng cho từng state
        switch (newState)
        {
            case GameState.Intro:
                // Giữ Intro cho đến khi người chơi tap Start Tile
                // StartPanelController sẽ gọi SetState(Playing) khi tap
                break;

            case GameState.Playing:
                float delay = gameConfig != null ? gameConfig.musicStartDelay : 0f;
                // MusicElapsed bắt đầu từ -delay (countdown), đến 0 nhạc mới phát
                _musicStartTime = Time.time + delay;
                _musicScheduled = false;
                // Việc phát nhạc thực sự sẽ do AudioManager lo (nghe event GameStateChanged)
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

    // ============================================================
    // DEBUG — Click chuột phải vào component trong Inspector
    // ============================================================

    [ContextMenu("Debug: Go to Playing")]
    private void DebugPlaying() => SetState(GameState.Playing);

    [ContextMenu("Debug: Trigger GameOver")]
    private void DebugGameOver() => TriggerGameOver();

    [ContextMenu("Debug: Go to CTA")]
    private void DebugCTA() => SetState(GameState.CTA);
}
