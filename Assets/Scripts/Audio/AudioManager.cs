using UnityEngine;

// ============================================================
// SFXType — Enum cho các loại sound effect
// ============================================================
public enum SFXType
{
    Tap,
    GoodHit,
    PerfectHit,
    GameOver,
    ButtonClick
}

// ============================================================
// AudioManager.cs — Quản lý nhạc nền và SFX
//
// - Phát AudioClip (mp3/ogg) khi nhấn Start Tile
// - Đồng bộ spawn tile qua audioSource.time
// - Phát SFX cho tap, good hit, perfect hit
// ============================================================
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("=== Music ===")]
    [Tooltip("AudioSource cho nhạc nền (cần Add AudioSource component)")]
    [SerializeField] private AudioSource musicSource;

    [Tooltip("AudioClip nhạc nền (kéo file mp3/ogg vào)")]
    [SerializeField] private AudioClip musicClip;

    [Header("=== SFX ===")]
    [Tooltip("AudioSource riêng cho SFX (để không conflict với nhạc)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("=== SFX Clips ===")]
    [SerializeField] private AudioClip sfxTap;
    [SerializeField] private AudioClip sfxGoodHit;
    [SerializeField] private AudioClip sfxPerfectHit;
    [SerializeField] private AudioClip sfxGameOver;
    [SerializeField] private AudioClip sfxButtonClick;

    [Header("=== Volume ===")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1.0f;

    /// <summary>
    /// Thời gian hiện tại của nhạc (giây).
    /// Dùng để đồng bộ spawn tile chính xác hơn Time.time
    /// </summary>
    public float MusicTime
    {
        get
        {
            if (musicSource != null && musicSource.isPlaying)
                return musicSource.time;
            return 0f;
        }
    }

    /// <summary>
    /// Nhạc đã kết thúc chưa?
    /// </summary>
    public bool IsMusicFinished
    {
        get
        {
            if (musicSource == null || musicClip == null) return true;
            return !musicSource.isPlaying && musicSource.time >= musicClip.length - 0.1f;
        }
    }

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

        SetupAudioSources();
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Update()
    {
        // Kiểm tra nhạc đã hết → trigger song complete
        if (musicSource != null && musicClip != null)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameState.Playing &&
                !musicSource.isPlaying &&
                musicSource.time == 0f) // AudioSource reset time to 0 khi hết
            {
                // Nhạc kết thúc → đợi tiles xong rồi TileSpawner sẽ trigger
                // Không trigger ở đây để tránh race condition
            }
        }
    }

    // ============================================================
    // SETUP
    // ============================================================
    private void SetupAudioSources()
    {
        // Tạo AudioSource cho nhạc nếu chưa có
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = false;
        }
        musicSource.volume = musicVolume;

        // Tạo AudioSource cho SFX nếu chưa có
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
        sfxSource.volume = sfxVolume;
    }

    // ============================================================
    // GAME STATE
    // ============================================================
    private void HandleGameStateChanged(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                PlayMusic();
                break;

            case GameState.GameOver:
                StopMusic();
                PlaySFX(SFXType.GameOver);
                break;

            case GameState.CTA:
                StopMusic();
                break;
        }
    }

    // ============================================================
    // MUSIC
    // ============================================================

    /// <summary>
    /// Bắt đầu phát nhạc nền
    /// </summary>
    public void PlayMusic()
    {
        if (musicSource == null || musicClip == null)
        {
            Debug.LogWarning("[AudioManager] Chưa assign musicClip!");
            return;
        }

        musicSource.clip = musicClip;
        musicSource.time = 0f;

        float delay = 0f;
        if (GameManager.Instance != null && GameManager.Instance.Config != null)
        {
            delay = GameManager.Instance.Config.musicStartDelay;
        }

        if (delay > 0f)
        {
            musicSource.PlayDelayed(delay);
            Debug.Log($"[AudioManager] Playing music: {musicClip.name} in {delay}s, length={musicClip.length:F1}s");
        }
        else
        {
            musicSource.Play();
            Debug.Log($"[AudioManager] Playing music: {musicClip.name}, length={musicClip.length:F1}s");
        }
    }

    /// <summary>
    /// Dừng nhạc
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Pause/Resume nhạc
    /// </summary>
    public void PauseMusic(bool pause)
    {
        if (musicSource == null) return;

        if (pause)
            musicSource.Pause();
        else
            musicSource.UnPause();
    }

    // ============================================================
    // SFX
    // ============================================================

    /// <summary>
    /// Phát sound effect
    /// </summary>
    public void PlaySFX(SFXType type)
    {
        AudioClip clip = GetSFXClip(type);

        if (clip == null) return;
        if (sfxSource == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    private AudioClip GetSFXClip(SFXType type)
    {
        switch (type)
        {
            case SFXType.Tap:        return sfxTap;
            case SFXType.GoodHit:    return sfxGoodHit;
            case SFXType.PerfectHit: return sfxPerfectHit;
            case SFXType.GameOver:   return sfxGameOver;
            case SFXType.ButtonClick: return sfxButtonClick;
            default: return null;
        }
    }
}
