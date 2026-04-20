using UnityEngine;

// ============================================================
// ScoreManager.cs — Visual feedback cho scoring
//
// Hiện tại: chuyển tiếp HitResult sang UIManager để cập nhật
// text "Good!" / "PERFECT!" ngay tại chỗ (không còn floating text).
// ============================================================
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

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

    // ============================================================
    // PUBLIC API
    // ============================================================

    /// <summary>
    /// Hiển thị kết quả hit: cập nhật text "Good!" / "PERFECT!" đứng yên trên UI.
    /// </summary>
    public void ShowHitFeedback(HitResult result, Vector3 worldPosition)
    {
        if (result == HitResult.None) return;
        if (UIManager.Instance != null)
            UIManager.Instance.ShowHitResultText(result);
    }
}
