using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// ============================================================
// UIManager.cs — Quản lý tất cả UI panels (Intro, Ingame, CTA)
//
// Canvas Scaler: Scale With Screen Size, ref 1080x1920, match 0.5
// ============================================================
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("=== Panels ===")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject ingamePanel;
    [SerializeField] private GameObject ctaPanel;

    [Header("=== Ingame UI ===")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Tooltip("Text hiển thị Good/PERFECT (cũ là comboText trên Canvas)")]
    [SerializeField] private TextMeshProUGUI hitResultText;

    [Header("=== Game Over ===")]
    [Tooltip("Delay (giây) trước khi chuyển sang CTA sau khi thua")]
    [SerializeField] private float gameOverToCTADelay = 1.5f;

    [Header("=== Intro UI ===")]
    [Tooltip("Headline image (Tagline_1.png)")]
    [SerializeField] private Image introHeadline;
    [Tooltip("Hand pointing image")]
    [SerializeField] private Image handPointing;

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

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
        GameManager.OnScoreChanged += UpdateScore;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
        GameManager.OnScoreChanged -= UpdateScore;
    }

    // ============================================================
    // STATE HANDLING
    // ============================================================
    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Intro:
                ShowPanel(introPanel);
                AnimateIntro();
                AnimateHandPointing(); // Hiệu ứng bàn tay phải được gọi ở Intro
                break;

            case GameState.Playing:
                ShowPanel(ingamePanel);
                HideHandPointing();
                // Reset hit result text khi bắt đầu chơi
                if (hitResultText != null)
                {
                    hitResultText.text = "";
                    hitResultText.gameObject.SetActive(false);
                }
                break;

            case GameState.GameOver:
                // Không có game over panel — chỉ delay rồi chuyển thẳng sang CTA
                // Dùng DOTween thay vì Invoke — không dùng string reflection
                DOVirtual.DelayedCall(gameOverToCTADelay, () =>
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.SetState(GameState.CTA);
                });
                break;

            case GameState.CTA:
                // Ẩn ingame, hiện CTA panel
                // BGEC background được OrientationManager bật (world-space)
                ShowPanel(ctaPanel);
                break;
        }
    }

    // ============================================================
    // PANEL MANAGEMENT
    // ============================================================
    private void ShowPanel(GameObject panel)
    {
        if (introPanel != null) introPanel.SetActive(false);
        if (ingamePanel != null) ingamePanel.SetActive(false);
        if (ctaPanel != null) ctaPanel.SetActive(false);

        if (panel != null) panel.SetActive(true);
    }

    // ============================================================
    // INTRO
    // ============================================================
    private void AnimateIntro()
    {
        if (introHeadline != null)
        {
            // Fade in headline
            introHeadline.color = new Color(1, 1, 1, 0);
            introHeadline.DOFade(1f, 0.5f).SetEase(Ease.OutCubic);

            // Scale punch
            introHeadline.transform.localScale = Vector3.one * 0.8f;
            introHeadline.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        }
    }

    // ============================================================
    // HAND POINTING (hướng dẫn tap)
    // ============================================================
    private void AnimateHandPointing()
    {
        if (handPointing == null) return;

        handPointing.gameObject.SetActive(true);

        // Animation di chuyển chéo (lên trên, sang trái) liên tục
        Vector3 targetPos = handPointing.transform.localPosition + new Vector3(-40f, 40f, 0f);
        handPointing.transform.DOLocalMove(targetPos, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void HideHandPointing()
    {
        if (handPointing == null) return;

        handPointing.transform.DOKill();
        handPointing.DOFade(0f, 0.3f).OnComplete(() =>
        {
            handPointing.gameObject.SetActive(false);
            // Reset alpha cho lần sau
            handPointing.color = Color.white;
        });
    }

    // ============================================================
    // SCORE UPDATE
    // ============================================================
    private void UpdateScore(int newScore)
    {
        if (scoreText != null)
            scoreText.text = newScore.ToString();
    }

    // ============================================================
    // HIT RESULT TEXT (thay thế combo text)
    // ============================================================

    [Header("=== Hit Result Colors ===")]
    [SerializeField] private Color goodColor    = new Color(0.2f, 0.95f, 0.3f, 1f);  // xanh lá
    [SerializeField] private Color perfectColor = new Color(1f,   0.85f, 0f,   1f);  // vàng gold

    /// <summary>
    /// Cập nhật text Good / PERFECT ngay tại chỗ (không bay lên).
    /// Gọi bởi ScoreManager.ShowHitFeedback().
    /// </summary>
    public void ShowHitResultText(HitResult result)
    {
        if (hitResultText == null) return;

        hitResultText.gameObject.SetActive(true);
        hitResultText.transform.DOKill();
        hitResultText.transform.localScale = Vector3.one;

        switch (result)
        {
            case HitResult.Perfect:
                hitResultText.text  = "PERFECT!";
                hitResultText.color = perfectColor;
                hitResultText.transform.DOPunchScale(Vector3.one * 0.4f, 0.25f, 6, 0.5f);
                break;

            case HitResult.Good:
                hitResultText.text  = "Good!";
                hitResultText.color = goodColor;
                hitResultText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 5, 0.5f);
                break;
        }
    }

}
