using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

// ============================================================
// InputHandler.cs — Xử lý Touch/Mouse input trên từng lane
//
// Sử dụng New Input System (EnhancedTouch API)
//
// Hỗ trợ:
// - Mobile: Multi-touch (mỗi ngón = 1 lane)
// - Desktop: Mouse click (1 input tại 1 thời điểm)
//
// Logic: Detect lane từ screen X → tìm tile trong lane đó
//        → gọi TileController.OnHoldStart() / OnHoldRelease()
// ============================================================
public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    [Header("=== Config ===")]
    [SerializeField] private GameConfig gameConfig;

    [Header("=== Camera ===")]
    [Tooltip("Main camera dùng để convert screen → world coords")]
    [SerializeField] private Camera mainCamera;

    // ---- Tracking active holds per finger/mouse ----
    // Key: fingerId (touch) hoặc -1 (mouse)
    // Value: TileController đang được hold
    private Dictionary<int, TileController> _activeHolds = new Dictionary<int, TileController>();

    private float _playStartTime;

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

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerUp += OnFingerUp;
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerUp -= OnFingerUp;
        EnhancedTouchSupport.Disable();
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        if (newState == GameState.Playing)
        {
            _playStartTime = Time.time;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        GameState state = GameManager.Instance.CurrentState;

        // Chỉ xử lý input khi Playing
        if (state != GameState.Playing)
            return;

        // Mouse input (Desktop / Editor) — dùng New Input System Mouse device
        HandleMouseInput();
    }

    // ============================================================
    // TOUCH INPUT (Mobile) — New Input System EnhancedTouch
    // ============================================================
    private void OnFingerDown(Finger finger)
    {
        if (GameManager.Instance == null) return;

        GameState state = GameManager.Instance.CurrentState;
        if (state != GameState.Playing)
            return;

        Vector2 screenPos = finger.screenPosition;
        int fingerId = finger.index;

        OnPointerDown(fingerId, screenPos);
    }

    private void OnFingerUp(Finger finger)
    {
        if (GameManager.Instance == null) return;

        int fingerId = finger.index;
        OnPointerUp(fingerId);
    }

    // ============================================================
    // MOUSE INPUT (Desktop / Editor) — New Input System
    // ============================================================
    private void HandleMouseInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        int mouseId = -1; // ID đặc biệt cho mouse

        if (mouse.leftButton.wasPressedThisFrame)
        {
            OnPointerDown(mouseId, mouse.position.ReadValue());
        }
        else if (mouse.leftButton.wasReleasedThisFrame)
        {
            OnPointerUp(mouseId);
        }
    }

    // ============================================================
    // CORE LOGIC
    // ============================================================

    /// <summary>
    /// Xử lý khi ngón tay / mouse bấm xuống
    /// </summary>
    private void OnPointerDown(int pointerId, Vector2 screenPos)
    {
        // Chỉ xử lý khi Playing
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        // Bỏ qua input trong 0.1s đầu tiên sau khi start game
        // để tránh race condition (tap Start Tile bị tính là tap nhầm lane)
        if (Time.time - _playStartTime < 0.1f)
            return;

        // Convert screen → world
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        // Xác định lane
        int lane = GetLaneFromWorldX(worldPos.x);
        if (lane < 0) return; // Tap ngoài vùng lane → bỏ qua (forgiving)

        // Tìm tile trong lane đang ở Hit Zone hoặc gần Hit Zone nhất
        TileController tile = FindTileForInteraction(lane);

        if (tile != null)
        {
            tile.OnHoldStart();
            _activeHolds[pointerId] = tile;
        }
        else
        {
            // Lane trống — bấm nhầm → chớp đỏ cảnh báo rồi Game Over
            if (LaneFlashEffect.Instance != null)
                LaneFlashEffect.Instance.FlashLane(lane);

            if (GameManager.Instance != null)
                GameManager.Instance.TriggerGameOver();
        }
    }

    /// <summary>
    /// Xử lý khi ngón tay / mouse nhả ra
    /// </summary>
    private void OnPointerUp(int pointerId)
    {
        if (!_activeHolds.ContainsKey(pointerId)) return;

        TileController tile = _activeHolds[pointerId];
        _activeHolds.Remove(pointerId);

        if (tile == null || !tile.gameObject.activeInHierarchy) return;

        // Nhả tay → kiểm tra kết quả
        HitResult result = tile.OnHoldRelease();

        switch (result)
        {
            case HitResult.Good:
                OnGoodHit(tile);
                break;

            case HitResult.Perfect:
                OnPerfectHit(tile);
                break;

            case HitResult.None:
                // Không có gì xảy ra (tile đã bị xử lý rồi)
                break;
        }
    }

    // ============================================================
    // HIT PROCESSING
    // ============================================================

    private void OnGoodHit(TileController tile)
    {
        if (gameConfig == null || GameManager.Instance == null) return;

        // Cộng điểm
        GameManager.Instance.AddScore(gameConfig.goodHitScore);

        // Floating text "Good!"
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ShowHitFeedback(HitResult.Good, tile.transform.position);

#if UNITY_EDITOR
        Debug.Log($"[InputHandler] GOOD HIT! Lane {tile.laneIndex}, +{gameConfig.goodHitScore}");
#endif
    }

    private void OnPerfectHit(TileController tile)
    {
        if (gameConfig == null || GameManager.Instance == null) return;

        // Cộng điểm
        GameManager.Instance.AddScore(gameConfig.perfectHitScore);

        // Floating text "PERFECT!"
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ShowHitFeedback(HitResult.Perfect, tile.transform.position);

#if UNITY_EDITOR
        Debug.Log($"[InputHandler] PERFECT HIT! Lane {tile.laneIndex}, +{gameConfig.perfectHitScore}");
#endif
    }


    // ============================================================
    // LANE DETECTION
    // ============================================================

    /// <summary>
    /// Xác định lane từ world X position.
    /// Trả về -1 nếu ngoài vùng lane.
    /// </summary>
    private int GetLaneFromWorldX(float worldX)
    {
        if (gameConfig == null) return -1;

        float minDist = float.MaxValue;
        int closestLane = -1;

        for (int i = 0; i < gameConfig.laneCount; i++)
        {
            // Dùng OrientationManager.GetLaneX() tính theo camera thực tế
            float laneX = OrientationManager.Instance != null
                ? OrientationManager.Instance.GetLaneX(i)
                : gameConfig.GetLaneX(i, false);

            float dist = Mathf.Abs(worldX - laneX);
            if (dist < minDist)
            {
                minDist = dist;
                closestLane = i;
            }
        }

        // Threshold = nửa lane width (forgiving: tap bất kỳ đâu trong lane là nhận)
        float halfLaneWidth = OrientationManager.Instance != null
            ? OrientationManager.Instance.GetLaneWidth() * 0.5f
            : 1.0f;

        if (minDist > halfLaneWidth)
            return -1;

        return closestLane;
    }

    // ============================================================
    // TILE FINDING
    // ============================================================

    /// <summary>
    /// Tìm tile thích hợp nhất trên lane để interact — single pass.
    /// </summary>
    private TileController FindTileForInteraction(int lane)
    {
        if (TilePool.Instance == null || GameManager.Instance == null) return null;
        
        TileController tile = TilePool.Instance.FindTileForLane(lane);
        
        if (tile != null && tile.tileState == TileState.Scrolling)
        {
            // Khoảng cách cho phép bấm sớm (forgiving window)
            // Nếu tile còn cao hơn mức này thì coi như bấm vào khoảng trống
            float forgivingTopY = GameManager.Instance.Config.hitWindowTop + 2.5f;
            
            if (tile.transform.position.y > forgivingTopY)
            {
                // Tile ở quá cao, chưa được phép bấm
                return null;
            }
        }
        
        return tile;
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    /// <summary>
    /// Xóa tất cả active holds (khi game over, reset)
    /// </summary>
    public void ClearAllHolds()
    {
        _activeHolds.Clear();
    }
}
