using UnityEngine;

// ============================================================
// TileHoldEffect.cs — Hiệu ứng đổ đầy màu vàng từ dưới lên
// Gắn lên root GameObject của Long Tile / Short Tile prefab.
//
// Yêu cầu Hierarchy trong prefab:
//   Long Tile (root)  ← TileHoldEffect gắn ở đây
//   └── Body (SpriteRenderer, pivot = Bottom: X=0.5, Y=0)
//       ├── FillMask  (SpriteMask, localScale.y bắt đầu = 0)
//       │   └── FillSprite (SpriteRenderer, color #FFD700,
//       │                   Mask Interaction = Visible Inside Mask)
//       └── ...
// ============================================================
public class TileHoldEffect : MonoBehaviour
{
    // ---- Inspector ----
    [Header("=== Hold Fill Effect ===")]
    [Tooltip("Kéo GameObject FillMask (child của Body) vào đây")]
    [SerializeField] private Transform fillMask;

    [Tooltip("Thời gian giữ để lấp đầy hoàn toàn (giây)")]
    [SerializeField] private float holdDuration = 1f;

    // ---- Runtime ----
    private float _progress;    // 0 → 1
    private bool  _holding;
    private bool  _completed;   // đã complete 1 lần, tránh gọi lại

    // ============================================================
    // PUBLIC API — gọi từ TileController
    // ============================================================

    /// <summary>
    /// Bắt đầu hiệu ứng fill (gọi khi OnHoldStart)
    /// </summary>
    public void StartHold()
    {
        if (fillMask == null) return;
        _holding   = true;
        _completed = false;
        _progress  = 0f;
        SetFillScale(0f);
    }

    /// <summary>
    /// Dừng animation nhưng GIỮ NGUYÊN vị trí màu vàng hiện tại.
    /// Gọi khi người chơi buông tay giữa chừng.
    /// </summary>
    public void PauseHold()
    {
        _holding = false;
        // _progress và fillMask.localScale GIỮ NGUYÊN (không reset)
    }

    /// <summary>
    /// Reset hoàn toàn về 0 — chỉ gọi khi trả tile về pool (ResetTile).
    /// </summary>
    public void ResetFill()
    {
        _holding   = false;
        _completed = false;
        _progress  = 0f;
        SetFillScale(0f);
    }

    /// <summary>
    /// Đặt holdDuration từ ngoài (TileController có thể truyền vào theo BPM)
    /// </summary>
    public void SetHoldDuration(float seconds)
    {
        holdDuration = Mathf.Max(0.1f, seconds);
    }

    /// <summary>
    /// True khi fill đã lấp đầy hoàn toàn
    /// </summary>
    public bool IsComplete => _completed;

    // ============================================================
    // UPDATE — Chạy animation fill frame-by-frame (NO Coroutines)
    // ============================================================
    private void Update()
    {
        if (!_holding || fillMask == null) return;

        _progress += Time.deltaTime / holdDuration;
        _progress  = Mathf.Clamp01(_progress);
        SetFillScale(_progress);

        if (_progress >= 1f && !_completed)
        {
            _completed = true;
            _holding   = false;
            OnFillComplete();
        }
    }

    // ============================================================
    // HELPERS
    // ============================================================
    private void SetFillScale(float t)
    {
        if (fillMask == null) return;
        // Chỉ thay đổi Y — mặt nạ "dâng" từ dưới lên
        Vector3 s = fillMask.localScale;
        s.y = t;
        fillMask.localScale = s;
    }

    private void OnFillComplete()
    {
        // Delegate sang TileController để đổi head → XX
        TileController tc = GetComponent<TileController>();
        if (tc != null)
            tc.OnFillComplete();

#if UNITY_EDITOR
        Debug.Log($"[TileHoldEffect] Fill complete on {gameObject.name}");
#endif
    }
}
