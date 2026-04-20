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
    [SerializeField] private TextMeshProUGUI comboText;

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
        GameManager.OnComboChanged += UpdateCombo;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
        GameManager.OnScoreChanged -= UpdateScore;
        GameManager.OnComboChanged -= UpdateCombo;
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
                break;

            case GameState.WaitingToStart:
                ShowPanel(ingamePanel);
                AnimateHandPointing();
                break;

            case GameState.Playing:
                ShowPanel(ingamePanel);
                HideHandPointing();
                break;

            case GameState.GameOver:
                // Không có game over panel — chỉ delay rồi chuyển thẳng sang CTA
                // BGEC background được OrientationManager bật khi GameOver
                Invoke(nameof(TransitionToCTA), gameOverToCTADelay);
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

        // Animation bob up-down liên tục
        handPointing.transform.DOLocalMoveY(
            handPointing.transform.localPosition.y + 30f, 0.5f)
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

    private void UpdateCombo(int newCombo)
    {
        if (comboText == null) return;

        if (newCombo > 1)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"x{newCombo}";

            // Punch scale trên mỗi combo mới
            comboText.transform.DOKill();
            comboText.transform.localScale = Vector3.one;
            comboText.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f);
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }

    // ============================================================
    // GAME OVER
    // ============================================================
    private void TransitionToCTA()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.CTA);
    }
}
