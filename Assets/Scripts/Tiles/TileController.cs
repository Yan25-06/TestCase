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

    [Header("=== Colors ===")]
    [Tooltip("Màu gốc của Body (hồng)")]
    public Color bodyOriginalColor = new Color(1f, 0.5f, 0.6f, 1f); // hồng nhạt

    [Tooltip("Màu khi đang hold (vàng)")]
    public Color bodyHoldColor = new Color(1f, 0.85f, 0f, 1f); // vàng

    // ---- Runtime State ----
    [HideInInspector] public TileState tileState = TileState.Idle;
    [HideInInspector] public int laneIndex;

    private float _scrollSpeed;
    private Sprite _originalHeadSprite;
    private GameConfig _config;
    private Tween _colorTween;

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

        // Reset body color
        if (bodyRenderer != null)
            bodyRenderer.color = bodyOriginalColor;

        // Đặt vị trí spawn
        float x = OrientationManager.Instance != null
            ? OrientationManager.Instance.GetLaneX(lane)
            : (config != null ? config.fallbackLaneXPositions[lane] : 0f);

        float y = config != null ? config.spawnY : 13f;

        transform.position = new Vector3(x, y, 0f);
        gameObject.SetActive(true);
    }

    // ============================================================
    // UPDATE — Di chuyển tile xuống
    // ============================================================
    private void Update()
    {
        if (tileState == TileState.Idle || tileState == TileState.Completed)
            return;

        // Chỉ scroll khi game đang Playing
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        // Cập nhật tốc độ từ GameManager (tăng dần theo score)
        _scrollSpeed = GameManager.Instance.CurrentScrollSpeed;

        // Di chuyển xuống
        transform.Translate(Vector3.down * _scrollSpeed * Time.deltaTime);

        // Check Hit Zone
        float tileBottomY = GetBottomY();
        if (_config != null)
        {
            // Kiểm tra tile vào Hit Zone
            if (tileState == TileState.Scrolling &&
                tileBottomY <= _config.hitWindowTop &&
                tileBottomY >= _config.hitWindowBottom)
            {
                tileState = TileState.InHitZone;
            }

            // Kiểm tra tile bị miss (trôi qua hết)
            if (tileBottomY < _config.missY && tileState != TileState.Completed)
            {
                OnMissed();
            }
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

        // Bắt đầu chuyển màu body Hồng → Vàng
        if (bodyRenderer != null)
        {
            _colorTween?.Kill();
            float duration = tileType == TileType.Long ? 1.5f : 0.5f;
            _colorTween = bodyRenderer.DOColor(bodyHoldColor, duration)
                .SetEase(Ease.Linear);
        }
    }

    /// <summary>
    /// Người chơi nhả tay — kiểm tra Good hay Perfect
    /// </summary>
    public HitResult OnHoldRelease()
    {
        _colorTween?.Kill();

        if (tileState != TileState.Holding)
            return HitResult.None;

        // Kiểm tra: Head đã vào Hit Zone chưa?
        bool headInZone = false;
        if (headRenderer != null && _config != null)
        {
            float headY = headRenderer.transform.position.y;
            headInZone = headY <= _config.hitWindowTop && headY >= _config.hitWindowBottom;
        }

        tileState = TileState.Completed;

        if (headInZone)
        {
            // PERFECT — đổi mặt Tam Giác → XX
            if (headRenderer != null && deadHeadSprite != null)
                headRenderer.sprite = deadHeadSprite;

            return HitResult.Perfect;
        }
        else
        {
            // GOOD — nhả sớm
            return HitResult.Good;
        }
    }

    /// <summary>
    /// Tile bị miss — trigger Game Over
    /// </summary>
    private void OnMissed()
    {
        tileState = TileState.Missed;

        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver();
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
        _colorTween?.Kill();

        // Reset visuals
        transform.localScale = Vector3.one;
        if (bodyRenderer != null)
        {
            bodyRenderer.color = bodyOriginalColor;
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
        // Dùng BoxCollider2D bounds nếu có
        var col = GetComponent<BoxCollider2D>();
        if (col != null)
            return col.bounds.min.y;

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
