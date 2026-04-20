using UnityEngine;
using DG.Tweening;

// ============================================================
// LaneFlashEffect.cs — Hiệu ứng chớp đỏ cảnh báo khi bấm nhầm lane trống
//
// Gắn lên một GameObject riêng trong Scene (ví dụ: "LaneFlashManager").
// Mỗi lane cần 1 SpriteRenderer overlay (full-height, same width as lane)
// đặt trên Sorting Layer cao hơn tile để đè lên tất cả.
// ============================================================
public class LaneFlashEffect : MonoBehaviour
{
    public static LaneFlashEffect Instance { get; private set; }

    [Header("=== Lane Overlays ===")]
    [Tooltip("Mỗi phần tử tương ứng với 1 lane (index khớp với laneIndex trong TileController)")]
    [SerializeField] private SpriteRenderer[] laneOverlays;

    [Header("=== Flash Settings ===")]
    [Tooltip("Màu chớp cảnh báo")]
    [SerializeField] private Color flashColor = new Color(1f, 0.1f, 0.1f, 0.75f);

    [Tooltip("Thời gian mỗi nhịp chớp (giây)")]
    [SerializeField] private float flashDuration = 0.08f;

    [Tooltip("Số lần chớp")]
    [SerializeField] private int flashCount = 2;

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

        // Đảm bảo tất cả overlay ẩn lúc đầu
        if (laneOverlays != null)
        {
            foreach (var sr in laneOverlays)
            {
                if (sr != null)
                    sr.color = Color.clear;
            }
        }
    }

    private void Start()
    {
        // Chờ các component khác init xong thì căn chỉnh kích thước
        UpdateOverlaysTransform();
    }

    private void OnEnable()
    {
        OrientationManager.OnOrientationChanged += HandleOrientationChanged;
    }

    private void OnDisable()
    {
        OrientationManager.OnOrientationChanged -= HandleOrientationChanged;
    }

    private void HandleOrientationChanged(bool isPortrait)
    {
        UpdateOverlaysTransform();
    }

    // ============================================================
    // DYNAMIC RESIZING
    // ============================================================

    /// <summary>
    /// Căn chỉnh lại vị trí và kích thước của các lane overlays
    /// để khớp với thiết bị và xoay ngang/dọc.
    /// </summary>
    private void UpdateOverlaysTransform()
    {
        if (laneOverlays == null || OrientationManager.Instance == null) return;
        
        Camera cam = Camera.main;
        if (cam == null) return;

        // Chiều cao camera trong world space
        float camHeight = cam.orthographicSize * 2f;
        // Chiều rộng của 1 lane
        float laneWidth = OrientationManager.Instance.GetLaneWidth();

        for (int i = 0; i < laneOverlays.Length; i++)
        {
            SpriteRenderer sr = laneOverlays[i];
            if (sr == null || sr.sprite == null) continue;

            // Tính vị trí X của lane thứ i
            float laneX = OrientationManager.Instance.GetLaneX(i);
            
            // Đặt vị trí vào giữa màn hình theo Y, và X tương ứng
            sr.transform.position = new Vector3(laneX, cam.transform.position.y, 0f);

            // Scale sprite cho khớp
            Vector2 spriteSize = sr.sprite.bounds.size;
            if (spriteSize.x > 0 && spriteSize.y > 0)
            {
                float scaleX = laneWidth / spriteSize.x;
                float scaleY = camHeight / spriteSize.y;
                sr.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
        }
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    /// <summary>
    /// Chạy hiệu ứng chớp đỏ trên lane chỉ định.
    /// Gọi bởi InputHandler khi phát hiện bấm nhầm lane trống.
    /// </summary>
    public void FlashLane(int laneIndex)
    {
        if (laneOverlays == null || laneIndex < 0 || laneIndex >= laneOverlays.Length)
            return;

        SpriteRenderer sr = laneOverlays[laneIndex];
        if (sr == null) return;

        // Đảm bảo luôn khớp kích thước trước khi chớp (phòng hờ chưa update)
        UpdateOverlaysTransform();

        // Kill tween cũ nếu đang chạy
        sr.DOKill();
        sr.color = Color.clear;

        // Sequence: chớp lên rồi tắt, lặp flashCount lần
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < flashCount; i++)
        {
            seq.Append(sr.DOColor(flashColor, flashDuration).SetEase(Ease.OutQuad));
            seq.Append(sr.DOColor(Color.clear, flashDuration).SetEase(Ease.InQuad));
        }
        seq.OnComplete(() => sr.color = Color.clear);
    }
}
