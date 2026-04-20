using UnityEngine;
using System;

// ============================================================
// OrientationManager.cs — Detect portrait/landscape,
// swap background, scale camera & lanes
// ============================================================
public class OrientationManager : MonoBehaviour
{
    public static OrientationManager Instance { get; private set; }

    // ---- Events ----
    public static event Action<bool> OnOrientationChanged; // true = portrait

    // ---- Inspector ----
    [Header("=== Camera ===")]
    [Tooltip("Main camera (auto-find nếu để trống)")]
    [SerializeField] private Camera mainCamera;

    [Header("=== Background Ingame ===")]
    [Tooltip("SpriteRenderer cho background Portrait 9:16")]
    [SerializeField] private SpriteRenderer bgIngamePortrait;

    [Tooltip("SpriteRenderer cho background Landscape 16:9")]
    [SerializeField] private SpriteRenderer bgIngameLandscape;

    [Header("=== Background CTA ===")]
    [Tooltip("SpriteRenderer cho background CTA Portrait 9:16")]
    [SerializeField] private SpriteRenderer bgCTAPortrait;

    [Tooltip("SpriteRenderer cho background CTA Landscape 16:9")]
    [SerializeField] private SpriteRenderer bgCTALandscape;

    [Header("=== Config ===")]
    [SerializeField] private GameConfig gameConfig;

    // ---- State ----
    private bool _isPortrait;
    public bool IsPortrait => _isPortrait;
    public bool IsLandscape => !_isPortrait;

    private bool _showingCTABg = false; // true khi đang hiện BGEC (GameOver/CTA)

    private int _lastScreenWidth;
    private int _lastScreenHeight;

    // ============================================================
    // LIFECYCLE
    // ============================================================
    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;

        // Detect lần đầu
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        _isPortrait = Screen.height > Screen.width;

        ApplyOrientation();
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Update()
    {
        // Chỉ check khi resolution thay đổi (tối ưu)
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            bool newIsPortrait = Screen.height > Screen.width;
            if (newIsPortrait != _isPortrait)
            {
                _isPortrait = newIsPortrait;
                ApplyOrientation();
                OnOrientationChanged?.Invoke(_isPortrait);
            }
        }
    }

    // ============================================================
    // GAME STATE — Switch background set
    // ============================================================
    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.CTA:
                // Chỉ khi vào CTA mới chuyển sang BGEC
                SwitchToBackground(useCTA: true);
                break;

            case GameState.Intro:
            case GameState.WaitingToStart:
            case GameState.Playing:
            case GameState.GameOver:
                // Giữ Ingame background (GameOver vẫn thấy ingame BG trong 1.5s)
                SwitchToBackground(useCTA: false);
                break;
        }
    }

    // ============================================================
    // APPLY
    // ============================================================
    private void ApplyOrientation()
    {
        ApplyCamera();
        ApplyBackgrounds();
    }

    /// <summary>
    /// Điều chỉnh camera orthographic size theo orientation
    /// </summary>
    private void ApplyCamera()
    {
        if (mainCamera == null || gameConfig == null) return;

        mainCamera.orthographicSize = _isPortrait
            ? gameConfig.portraitCameraSize
            : gameConfig.landscapeCameraSize;
    }

    /// <summary>
    /// Enable/disable background sprites theo orientation VÀ game state.
    /// Ingame BG: Intro / WaitingToStart / Playing
    /// CTA BG:    GameOver / CTA
    /// </summary>
    private void ApplyBackgrounds()
    {
        bool showIngame = !_showingCTABg;
        bool showCTA    = _showingCTABg;

        // Ingame backgrounds (chỉ hiện khi showIngame và đúng orientation)
        if (bgIngamePortrait != null)
            bgIngamePortrait.enabled  = showIngame && _isPortrait;
        if (bgIngameLandscape != null)
            bgIngameLandscape.enabled = showIngame && !_isPortrait;

        // CTA backgrounds (BGEC_Squid_Game_9x16 / 16x9)
        if (bgCTAPortrait != null)
            bgCTAPortrait.enabled  = showCTA && _isPortrait;
        if (bgCTALandscape != null)
            bgCTALandscape.enabled = showCTA && !_isPortrait;
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    /// <summary>
    /// Chuyển sang bộ background tương ứng với game state.
    /// useCTA = true  → BGEC_Squid_Game (GameOver / CTA screen)
    /// useCTA = false → Ingame background (Playing)
    /// Portrait/Landscape swap vẫn được giữ nguyên.
    /// </summary>
    public void SwitchToBackground(bool useCTA)
    {
        _showingCTABg = useCTA;
        ApplyBackgrounds();

        Debug.Log($"[OrientationManager] Background → {(useCTA ? "CTA (BGEC)" : "Ingame")}, portrait={_isPortrait}");
    }

    /// <summary>
    /// Lấy độ rộng có thể chơi được (chứa các lane).
    /// Giới hạn tối đa bằng độ rộng của background hiện tại để tránh lane bị tràn ra ngoài 
    /// trên các màn hình quá rộng (ví dụ iPad 4:3).
    /// Đối với màn hình ngang (desktop), chỉ lấy 1/4 màn hình ở giữa.
    /// </summary>
    private float GetPlayableWidth()
    {
        if (mainCamera == null) return 1f;

        float camWidth = mainCamera.orthographicSize * mainCamera.aspect * 2f;

        if (_isPortrait)
        {
            // Màn hình dọc: Giới hạn tối đa bằng độ rộng background dọc
            if (bgIngamePortrait != null && bgIngamePortrait.sprite != null)
            {
                float bgWidth = bgIngamePortrait.bounds.size.x;
                return Mathf.Min(camWidth, bgWidth);
            }
        }
        else
        {
            // Màn hình ngang: Chỉ lấy phần trung tâm (khoảng 1/4 màn hình)
            return camWidth / 4f;
        }

        return camWidth;
    }

    /// <summary>
    /// Lấy vị trí X của lane — tính động từ độ rộng màn hình, giới hạn bởi background.
    /// </summary>
    public float GetLaneX(int laneIndex)
    {
        if (mainCamera == null || gameConfig == null)
        {
            // Fallback khi chưa setup
            return gameConfig != null
                ? gameConfig.GetLaneX(laneIndex, IsLandscape)
                : 0f;
        }

        int count = gameConfig.laneCount;
        if (laneIndex < 0 || laneIndex >= count) return 0f;

        float totalWidth = GetPlayableWidth();
        float laneWidth  = totalWidth / count;

        // Center của lane = cạnh trái + (laneIndex + 0.5) * laneWidth
        float laneCenter = -(totalWidth / 2f) + (laneIndex + 0.5f) * laneWidth;

        return laneCenter;
    }

    /// <summary>
    /// Chiều rộng mỗi lane trong world units
    /// </summary>
    public float GetLaneWidth()
    {
        if (mainCamera == null || gameConfig == null) return 1f;

        return GetPlayableWidth() / gameConfig.laneCount;
    }

    /// <summary>
    /// Scale background cho vừa camera viewport
    /// Gọi sau khi thay đổi sprite nếu cần
    /// </summary>
    public void FitBackgroundToCamera(SpriteRenderer sr)
    {
        if (sr == null || sr.sprite == null || mainCamera == null) return;

        float worldScreenHeight = mainCamera.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * mainCamera.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;

        float scaleX = worldScreenWidth / spriteSize.x;
        float scaleY = worldScreenHeight / spriteSize.y;
        float scale = Mathf.Max(scaleX, scaleY); // Cover mode

        sr.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
