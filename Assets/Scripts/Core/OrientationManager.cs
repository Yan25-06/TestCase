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
    /// Enable/disable background sprites theo orientation
    /// </summary>
    private void ApplyBackgrounds()
    {
        // Ingame backgrounds
        if (bgIngamePortrait != null)
            bgIngamePortrait.enabled = _isPortrait;
        if (bgIngameLandscape != null)
            bgIngameLandscape.enabled = !_isPortrait;

        // CTA backgrounds
        if (bgCTAPortrait != null)
            bgCTAPortrait.enabled = _isPortrait;
        if (bgCTALandscape != null)
            bgCTALandscape.enabled = !_isPortrait;
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    /// <summary>
    /// Lấy vị trí X của lane có tính orientation
    /// </summary>
    public float GetLaneX(int laneIndex)
    {
        if (gameConfig == null) return 0f;
        return gameConfig.GetLaneX(laneIndex, IsLandscape);
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
