using UnityEngine;
using DG.Tweening;
using TMPro;

// ============================================================
// ScoreManager.cs — Quản lý visual feedback cho scoring
//
// - Spawn floating text "Good!" / "PERFECT!"
// - Hiệu ứng DOTween scale + fade
// - Quản lý pool floating text objects
// ============================================================
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("=== Floating Text ===")]
    [Tooltip("Prefab cho floating text (cần tạo: TextMeshPro object)")]
    [SerializeField] private GameObject floatingTextPrefab;

    [Tooltip("Offset Y khi spawn text (phía trên tile)")]
    [SerializeField] private float textOffsetY = 1.5f;

    [Tooltip("Thời gian bay lên + fade")]
    [SerializeField] private float textDuration = 0.8f;

    [Tooltip("Khoảng cách bay lên")]
    [SerializeField] private float textRiseDistance = 2f;

    [Header("=== Colors ===")]
    [SerializeField] private Color goodColor = new Color(0.2f, 0.85f, 0.2f, 1f); // Xanh lá
    [SerializeField] private Color perfectColor = new Color(1f, 0.85f, 0f, 1f);  // Vàng gold

    [Header("=== Screen Shake ===")]
    [Tooltip("Camera để shake khi Perfect")]
    [SerializeField] private Camera mainCamera;

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

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    /// <summary>
    /// Hiển thị floating text feedback tại vị trí tile
    /// </summary>
    public void ShowHitFeedback(HitResult result, Vector3 worldPosition)
    {
        if (result == HitResult.None) return;

        string text;
        Color color;
        float scale;

        switch (result)
        {
            case HitResult.Perfect:
                text = "PERFECT!";
                color = perfectColor;
                scale = 1.5f;
                // Screen shake cho Perfect
                ShakeCamera();
                break;

            case HitResult.Good:
            default:
                text = "Good!";
                color = goodColor;
                scale = 1.0f;
                break;
        }

        SpawnFloatingText(text, color, scale, worldPosition);
    }

    // ============================================================
    // FLOATING TEXT
    // ============================================================

    /// <summary>
    /// Spawn floating text với DOTween animation
    /// </summary>
    private void SpawnFloatingText(string text, Color color, float startScale, Vector3 worldPos)
    {
        // Vị trí spawn (phía trên tile)
        Vector3 spawnPos = worldPos + Vector3.up * textOffsetY;

        GameObject textObj;
        TextMeshPro tmp;

        if (floatingTextPrefab != null)
        {
            // Dùng prefab
            textObj = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity, transform);
            tmp = textObj.GetComponent<TextMeshPro>();
        }
        else
        {
            // Fallback: tạo runtime (cho testing khi chưa có prefab)
            textObj = new GameObject("FloatingText");
            textObj.transform.position = spawnPos;
            textObj.transform.SetParent(transform);
            tmp = textObj.AddComponent<TextMeshPro>();
            tmp.fontSize = 8f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 100;
        }

        if (tmp == null)
        {
            Destroy(textObj);
            return;
        }

        // Setup text
        tmp.text = text;
        tmp.color = color;
        textObj.transform.localScale = Vector3.zero;

        // Animation: punch scale → fly up → fade out
        Sequence seq = DOTween.Sequence();

        // Punch scale lên
        seq.Append(textObj.transform.DOScale(Vector3.one * startScale, 0.15f)
            .SetEase(Ease.OutBack));

        // Bay lên + fade out
        seq.Append(textObj.transform.DOMoveY(spawnPos.y + textRiseDistance, textDuration)
            .SetEase(Ease.OutQuad));
        seq.Join(tmp.DOFade(0f, textDuration).SetEase(Ease.InQuad));

        // Destroy khi xong
        seq.OnComplete(() => Destroy(textObj));
    }

    // ============================================================
    // SCREEN SHAKE
    // ============================================================

    /// <summary>
    /// Shake camera nhẹ khi Perfect Hit
    /// </summary>
    private void ShakeCamera()
    {
        if (mainCamera == null) return;

        // Kill any existing shake trước
        mainCamera.transform.DOKill();

        mainCamera.transform.DOShakePosition(
            shakeDuration,
            shakeStrength,
            vibrato: 10,
            randomness: 90,
            fadeOut: true
        );
    }
}
