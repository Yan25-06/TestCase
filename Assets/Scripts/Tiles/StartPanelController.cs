using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

// ============================================================
// StartPanelController.cs — Logic riêng cho Start Tile
// Gắn lên Start Tile prefab (đã có sẵn reference trong prefab,
// guid: c9bc85c6b91e31845965abaeb79d4803)
//
// Khi người chơi tap vào Start Tile → bắt đầu game
// Dùng IPointerClickHandler (EventSystem) thay vì Physics2D.Raycast
// ============================================================
public class StartPanelController : MonoBehaviour, IPointerClickHandler
{
    [Header("=== References ===")]
    [Tooltip("TileController trên cùng GameObject (auto-get)")]
    [SerializeField] private TileController tileController;

    private bool _tapped = false;
    private Tween _breathingTween;

    private void Awake()
    {
        if (tileController == null)
            tileController = GetComponent<TileController>();
    }

    private void OnEnable()
    {
        _tapped = false;
        
        // Hoạt ảnh nhịp thở (Breathing)
        transform.localScale = Vector3.one;
        _breathingTween = transform.DOScale(1.05f, 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnDisable()
    {
        _breathingTween?.Kill();
        transform.localScale = Vector3.one;
    }

    // ============================================================
    // Gọi khi người chơi chạm vào Start Tile
    // ============================================================

    /// <summary>
    /// Xử lý tap vào Start Tile.
    /// Gọi bởi InputHandler khi detect tap trên Start Tile.
    /// </summary>
    public void OnTap()
    {
        if (_tapped) return;
        if (GameManager.Instance == null) return;

        // Ch\u1ec9 cho ph\u00e9p tap t\u1eeb Intro (nh\u1ea3y th\u1eb3ng Playing, b\u1ecf qua WaitingToStart)
        GameState state = GameManager.Instance.CurrentState;
        if (state != GameState.Intro) return;

        _tapped = true;

#if UNITY_EDITOR
        Debug.Log($"[StartPanelController] Start Tile tapped from {state} \u2192 Playing");
#endif

        // Phát âm thanh ngay lập tức để ép trình duyệt "Mở Khóa" (Unlock) Web Audio API
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SFXType.Tap);
        }

        // Hủy breathing
        _breathingTween?.Kill();

        // QUAN TRỌNG CHO WEBGL MOBILE: Phải đổi state (và phát nhạc) NGAY LẬP TỨC 
        // trong cùng 1 frame của sự kiện Click. Nếu để trong OnComplete (bị delay), 
        // trình duyệt di động (Safari/Chrome) sẽ chặn nhạc vì cho rằng không có tương tác người dùng.
        GameManager.Instance.SetState(GameState.Playing);

        // Hoạt ảnh Squish (Bóp méo) trước khi bắt đầu
        transform.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.15f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Animation biến mất cho Start Tile
                if (tileController != null)
                {
                    tileController.tileState = TileState.Completed;
                    tileController.PlayCompletionAnimation();
                }
                else
                {
                    gameObject.SetActive(false);
                }
            });
    }

    // IPointerClickHandler: Unity EventSystem gọi khi có click/tap
    // Cần: Physics2DRaycaster trên Camera và Collider2D trên tile
    public void OnPointerClick(PointerEventData eventData)
    {
        OnTap();
    }

    // OnMouseDown: fallback khi chạy trong Editor không có EventSystem
    private void OnMouseDown()
    {
        OnTap();
    }
}
