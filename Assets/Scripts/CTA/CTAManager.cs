using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// ============================================================
// CTAManager.cs — Màn hình CTA (Call-To-Action)
//
// Hiển thị background EC, headline, game info, nút download
// Nút mở store link (Application.OpenURL)
// ============================================================
public class CTAManager : MonoBehaviour
{
    public static CTAManager Instance { get; private set; }

    [Header("=== CTA Elements ===")]
    [Tooltip("Nút Download/CTA (EC_button.png)")]
    [SerializeField] private Button ctaButton;

    [Tooltip("Image EC Headline")]
    [SerializeField] private Image ecHeadline;

    [Tooltip("Image Game Info")]
    [SerializeField] private Image gameInfo;

    [Tooltip("Final Score Text (nếu có)")]
    [SerializeField] private TMPro.TextMeshProUGUI finalScoreText;

    [Header("=== Store Link ===")]
    [Tooltip("URL mở khi nhấn nút CTA")]
    [SerializeField] private string storeURL = "https://play.google.com/store";

    [Header("=== Animation ===")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float buttonPulseScale = 1.1f;
    [SerializeField] private float buttonPulseDuration = 0.8f;

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

        // Gắn onClick cho button
        if (ctaButton != null)
            ctaButton.onClick.AddListener(OnCTAButtonClicked);
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;

        if (ctaButton != null)
            ctaButton.onClick.RemoveListener(OnCTAButtonClicked);
    }

    // ============================================================
    // STATE HANDLING
    // ============================================================
    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        if (newState == GameState.CTA)
        {
            AnimateCTAScreen();
        }
    }

    // ============================================================
    // ANIMATION
    // ============================================================
    private void AnimateCTAScreen()
    {
        // Hiện final score
        if (finalScoreText != null && GameManager.Instance != null)
        {
            finalScoreText.text = $"{GameManager.Instance.Score}";
        }

        // Fade in headline
        if (ecHeadline != null)
        {
            ecHeadline.color = new Color(1, 1, 1, 0);
            ecHeadline.DOFade(1f, fadeInDuration).SetEase(Ease.OutCubic);
            ecHeadline.transform.localScale = Vector3.one * 0.5f;
            ecHeadline.transform.DOScale(1f, fadeInDuration).SetEase(Ease.OutBack);
        }

        // Fade in game info
        if (gameInfo != null)
        {
            gameInfo.color = new Color(1, 1, 1, 0);
            gameInfo.DOFade(1f, fadeInDuration).SetDelay(0.2f).SetEase(Ease.OutCubic);
        }

        // Pulse animation trên nút CTA (loop vô hạn)
        if (ctaButton != null)
        {
            // Fade in button
            Image btnImage = ctaButton.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = new Color(1, 1, 1, 0);
                btnImage.DOFade(1f, fadeInDuration).SetDelay(0.3f);
            }

            // Pulse scale
            ctaButton.transform.DOScale(buttonPulseScale, buttonPulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetDelay(fadeInDuration + 0.3f);
        }

        // SFX
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFXType.ButtonClick);

        Debug.Log("[CTAManager] CTA Screen displayed");
    }

    // ============================================================
    // BUTTON
    // ============================================================
    private void OnCTAButtonClicked()
    {
        Debug.Log($"[CTAManager] CTA Button clicked → Opening: {storeURL}");

        // Mở store link
        Application.OpenURL(storeURL);

        // SFX
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFXType.ButtonClick);
    }
}
