using UnityEngine;
using DG.Tweening;

// ============================================================
// ScoreManager.cs — Visual feedback cho scoring
//
// Hiện tại: chuyển tiếp HitResult sang UIManager để cập nhật
// text "Good!" / "PERFECT!" ngay tại chỗ (không còn floating text).
// ============================================================
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("=== Screen Shake ===")]
    [SerializeField] private float shakeStrength = 0.3f;
    [SerializeField] private float shakeDuration = 0.2f;

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

    // ============================================================
    // SCREEN SHAKE
    // ============================================================

    /// <summary>
    /// Shake camera nhẹ khi Perfect Hit (head đổi thành XX)
    /// </summary>
    public void ShakeCamera()
    {
        if (Camera.main == null) return;

        // Kill any existing shake trước
        Camera.main.transform.DOKill();

        Camera.main.transform.DOShakePosition(
            shakeDuration,
            shakeStrength,
            vibrato: 10,
            randomness: 90,
            fadeOut: true
        );
    }
}
