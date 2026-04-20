using UnityEngine;

// ============================================================
// StartPanelController.cs — Logic riêng cho Start Tile
// Gắn lên Start Tile prefab (đã có sẵn reference trong prefab,
// guid: c9bc85c6b91e31845965abaeb79d4803)
//
// Khi người chơi tap vào Start Tile → bắt đầu game
// ============================================================
public class StartPanelController : MonoBehaviour
{
    [Header("=== References ===")]
    [Tooltip("TileController trên cùng GameObject (auto-get)")]
    [SerializeField] private TileController tileController;

    private bool _tapped = false;

    private void Awake()
    {
        if (tileController == null)
            tileController = GetComponent<TileController>();
    }

    private void OnEnable()
    {
        _tapped = false;
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
        if (state != GameState.Intro && state != GameState.WaitingToStart) return;

        _tapped = true;

        Debug.Log($"[StartPanelController] Start Tile tapped from {state} \u2192 Playing");

        // Chuy\u1ec3n game state \u2192 Playing (s\u1ebd trigger nh\u1ea1c + spawn)
        GameManager.Instance.SetState(GameState.Playing);

        // Animation bi\u1ebfn m\u1ea5t cho Start Tile
        if (tileController != null)
        {
            tileController.tileState = TileState.Completed;
            tileController.PlayCompletionAnimation();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // ============================================================
    // Detect tap bằng Unity 2D physics (backup)
    // Nếu InputHandler không handle, dùng OnMouseDown
    // ============================================================
    private void OnMouseDown()
    {
        OnTap();
    }
}
