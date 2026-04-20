using UnityEngine;
using DG.Tweening;

// ============================================================
// TileController.cs — Controller chung cho Start/Short/Long Tile
// Gắn lên root GameObject của mỗi prefab.
//
// ⚠️ Class name PHẢI là "TileController" vì 3 prefab đã
//    reference Assembly-CSharp::TileController (guid: ed10fd9...)
// ============================================================
public class TileController : MonoBehaviour
{
    // ---- Inspector (đã assign trong Prefab) ----
    [Header("=== Tile Config ===")]
    [Tooltip("Loại tile: Start=0, Short=1, Long=2")]
    public TileType tileType;

    [Tooltip("SpriteRenderer của Head (chỉ Short/Long có)")]
    public SpriteRenderer headRenderer;

    [Tooltip("Sprite 'XX dead' thay thế khi Perfect Hit")]
    public Sprite deadHeadSprite;

    // ---- Inspector (cần assign thêm) ----
    [Header("=== Body ===")]
    [Tooltip("SpriteRenderer của Body (kéo child Body vào đây)")]
    public SpriteRenderer bodyRenderer;

    // ---- Runtime State ----
    [HideInInspector] public TileState tileState = TileState.Idle;
    [HideInInspector] public int laneIndex;

    private float _scrollSpeed;
    private Sprite _originalHeadSprite;
    private GameConfig _config;
    private bool _headSwapped = false; // true khi head đã đổi sang XX
    private BoxCollider2D _collider; // cached — tránh GetComponent mỗi frame
    private TileHoldEffect _holdEffect;  // fill-from-bottom effect component

    // ============================================================
    // LIFECYCLE
    // ============================================================
    private void Start()
    {
        // Cache collider một lần duy nhất
        _collider = GetComponent<BoxCollider2D>();

        // Cache TileHoldEffect (gắn cùng root)
        _holdEffect = GetComponent<TileHoldEffect>();

        // Auto-tìm bodyRenderer từ child "Body" nếu chưa assign trong Inspector
        if (bodyRenderer == null)
        {
            Transform bodyChild = transform.Find("Body");
            if (bodyChild != null)
                bodyRenderer = bodyChild.GetComponent<SpriteRenderer>();
        }
    }

    // ============================================================
    // INIT — Gọi bởi TileSpawner khi spawn từ pool
    // ============================================================

    /// <summary>
    /// Khởi tạo tile khi được spawn (lấy từ pool)
    /// </summary>
    public void Init(int lane, float speed, GameConfig config)
    {
        laneIndex = lane;
        _scrollSpeed = speed;
        _config = config;
        tileState = TileState.Scrolling;

        // Lưu sprite gốc của head để reset khi recycle
        if (headRenderer != null)
            _originalHeadSprite = headRenderer.sprite;

        // Đặt vị trí spawn
        float x = OrientationManager.Instance != null
            ? OrientationManager.Instance.GetLaneX(lane)
            : (config != null ? config.fallbackLaneXPositions[lane] : 0f);

        float y = config != null ? config.spawnY : 13f;

        transform.position = new Vector3(x, y, 0f);

        // Tự động scale tile cho vừa với độ rộng của Lane
        if (OrientationManager.Instance != null && bodyRenderer != null && bodyRenderer.sprite != null)
        {
            float laneWidth = OrientationManager.Instance.GetLaneWidth();
            float padding = config != null ? config.horizontalPadding : 0f;
            float targetWidth = laneWidth - (padding * 2f);

            float spriteWidth = bodyRenderer.sprite.bounds.size.x;
            if (spriteWidth > 0f)
            {
                float scaleX = targetWidth / spriteWidth;
                
                // Áp dụng uniform scale để tránh làm méo ảnh (đặc biệt là phần Head)
                transform.localScale = new Vector3(scaleX, scaleX, 1f);
            }
        }

        gameObject.SetActive(true);
    }

    // ============================================================
    // UPDATE — Di chuyển tile xuống
    // ============================================================
    private void Update()
    {
        // Idle: trong pool, không làm gì
        if (tileState == TileState.Idle) return;

        // Chỉ scroll khi game đang Playing
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        // Cập nhật tốc độ từ GameManager (tăng dần theo score)
        _scrollSpeed = GameManager.Instance.CurrentScrollSpeed;

        // Di chuyển xuống — kể cả Completed tile vẫn tiếp tục rơi
        transform.Translate(Vector3.down * _scrollSpeed * Time.deltaTime);

        float tileBottomY = GetBottomY();
        float headY = GetHeadY();
        
        // Tính toán cạnh dưới của màn hình dựa vào Camera (linh hoạt theo thiết bị)
        float screenBottomY = -10f;
        if (Camera.main != null)
        {
            screenBottomY = Camera.main.transform.position.y - Camera.main.orthographicSize;
        }

        if (_config != null)
        {
            if (tileState == TileState.Completed || tileState == TileState.Missed)
            {
                // Trả về pool khi TOÀN BỘ tile (headY) đã rơi khỏi màn hình hoàn toàn
                if (headY < screenBottomY - 1.5f)
                {
                    if (TilePool.Instance != null)
                        TilePool.Instance.Return(this);
                }
                return;
            }

            // Kiểm tra tile vào Hit Zone
            if (tileState == TileState.Scrolling &&
                tileBottomY <= _config.hitWindowTop &&
                tileBottomY >= _config.hitWindowBottom)
            {
                tileState = TileState.InHitZone;
            }

            // Đang hold + head vào Hit Zone → tự động đổi head sang XX (Perfect)
            if (tileState == TileState.Holding && !_headSwapped)
            {
                CheckAutoHeadSwap();
            }

            // Kiểm tra tile bị miss (trôi qua màn hình mà chưa được hit)
            // Linh hoạt theo chiều dài: Đợi đỉnh của tile (headY) tụt hẳn dưới màn hình mới báo Miss.
            // (hoặc nếu là Long Tile, bạn có thể kiểm tra tileBottomY < hitWindowBottom nếu muốn báo Miss sớm)
            // Ở đây ta dùng headY < screenBottomY để tile kịp trôi hết khỏi màn hình.
            if (headY < screenBottomY)
            {
                OnMissed();
            }
        }
    }

    /// <summary>
    /// Kiểm tra khi đang hold: nếu head đã chạm Hit Zone → tự đổi mặt sang XX
    /// </summary>
    private void CheckAutoHeadSwap()
    {
        if (headRenderer == null || _config == null) return;

        float headY = headRenderer.transform.position.y;
        if (headY <= _config.hitWindowTop && headY >= _config.hitWindowBottom)
        {
            // PERFECT — đổi mặt Tam Giác → XX ngay lập tức
            _headSwapped = true;
            if (deadHeadSprite != null)
                headRenderer.sprite = deadHeadSprite;

#if UNITY_EDITOR
            Debug.Log($"[TileController] Head swapped to XX! Lane {laneIndex}");
#endif
        }
    }

    // ============================================================
    // INTERACTION — Gọi bởi InputHandler
    // ============================================================

    /// <summary>
    /// Người chơi bắt đầu hold tile này
    /// </summary>
    public void OnHoldStart()
    {
        if (tileState != TileState.InHitZone && tileState != TileState.Scrolling)
            return;

        tileState = TileState.Holding;

        // Kích hoạt hiệu ứng fill từ dưới lên (nếu có TileHoldEffect gắn trên tile)
        if (_holdEffect != null)
        {
            _holdEffect.StartHold();
        }
    }

    /// <summary>
    /// Người chơi nhả tay — kiểm tra Good hay Perfect
    /// Kết quả dựa trên _headSwapped (head đã tự đổi trong Update chưa)
    /// </summary>
    public HitResult OnHoldRelease()
    {
        // Dừng animation, giữ nguyên vị trí màu vàng hiện tại
        if (_holdEffect != null && !_holdEffect.IsComplete)
            _holdEffect.PauseHold();

        if (tileState != TileState.Holding)
            return HitResult.None;

        tileState = TileState.Completed;

        if (_headSwapped)
        {
            // Head đã được swap → PERFECT
            return HitResult.Perfect;
        }
        else
        {
            // Nhả trước khi head vào zone → GOOD
            return HitResult.Good;
        }
    }

    /// <summary>
    /// Callback từ TileHoldEffect khi fill đã lấp đầy 100%.
    /// Đổi head sang sprite XX ngay lập tức.
    /// </summary>
    public void OnFillComplete()
    {
        if (headRenderer != null && deadHeadSprite != null && !_headSwapped)
        {
            _headSwapped = true;
            headRenderer.sprite = deadHeadSprite;
        }

#if UNITY_EDITOR
        Debug.Log($"[TileController] OnFillComplete → head swapped to XX. Lane {laneIndex}");
#endif
    }

    /// <summary>
    /// Tile bị miss — trigger Game Over
    /// </summary>
    private void OnMissed()
    {
        tileState = TileState.Missed;
#if UNITY_EDITOR
        Debug.Log($"[TileController] Tile missed on lane {laneIndex}");
#endif

        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver();

        // Tự động thu hồi tile về pool để không bị "rác" màn hình khi test
        // if (TilePool.Instance != null)
        //     TilePool.Instance.Return(this);
    }

    // ============================================================
    // ANIMATION — Hoàn tất sau khi hit
    // ============================================================

    /// <summary>
    /// Animation fade out + thu nhỏ sau khi xử lý xong
    /// </summary>
    public void PlayCompletionAnimation(System.Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();

        // Scale down + fade
        seq.Append(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));

        if (bodyRenderer != null)
            seq.Join(bodyRenderer.DOFade(0f, 0.3f));
        if (headRenderer != null)
            seq.Join(headRenderer.DOFade(0f, 0.3f));

        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
            ResetTile();
        });
    }

    // ============================================================
    // POOL — Reset & Recycle
    // ============================================================

    /// <summary>
    /// Reset tile về trạng thái ban đầu, deactivate (trả pool)
    /// </summary>
    public void ResetTile()
    {
        tileState = TileState.Idle;
        _headSwapped = false;

        // Reset fill effect hoàn toàn (trả pool)
        if (_holdEffect != null)
            _holdEffect.ResetFill();

        // Reset visuals
        transform.localScale = Vector3.one;
        if (bodyRenderer != null)
        {
            var c = bodyRenderer.color; c.a = 1f; bodyRenderer.color = c;
        }
        if (headRenderer != null)
        {
            if (_originalHeadSprite != null)
                headRenderer.sprite = _originalHeadSprite;
            var c = headRenderer.color; c.a = 1f; headRenderer.color = c;
        }

        gameObject.SetActive(false);
    }

    // ============================================================
    // HELPERS
    // ============================================================

    /// <summary>
    /// Lấy Y thấp nhất của tile (bottom edge)
    /// </summary>
    private float GetBottomY()
    {
        // Dùng cached _collider thay vì GetComponent mỗi frame
        if (_collider != null)
            return _collider.bounds.min.y;

        // Fallback: dùng transform.position
        return transform.position.y;
    }

    /// <summary>
    /// Lấy Y của Head
    /// </summary>
    public float GetHeadY()
    {
        if (headRenderer != null)
            return headRenderer.transform.position.y;
        return transform.position.y;
    }
}

// ============================================================
// HitResult — Kết quả khi nhả tay
// ============================================================
public enum HitResult
{
    None,
    Good,
    Perfect
}
