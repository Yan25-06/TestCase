using UnityEngine;

public class LaneBackgroundLayout : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SpriteRenderer[] laneRenderers = new SpriteRenderer[4];

    [Header("Behavior")]
    [SerializeField] private bool updateWhenScreenResized = true;
    [SerializeField] private bool stretchToCameraHeight = true;

    private Vector3[] baseScales;
    private int cachedScreenWidth;
    private int cachedScreenHeight;

    private void Awake()
    {
        CacheBaseScales();
        ApplyLayout();
        CacheScreenSize();
    }

    private void OnEnable()
    {
        ApplyLayout();
        CacheScreenSize();
    }

    private void Update()
    {
        if (!updateWhenScreenResized)
        {
            return;
        }

        if (cachedScreenWidth == Screen.width && cachedScreenHeight == Screen.height)
        {
            return;
        }

        ApplyLayout();
        CacheScreenSize();
    }

    [ContextMenu("Apply Lane Layout")]
    public void ApplyLayout()
    {
        GameConfig config = ResolveConfig();
        Camera cam = ResolveCamera();

        if (config == null || cam == null)
        {
            return;
        }

        config.RecalculateLaneLayout(cam);

        float laneWidth = config.LaneWidth;
        if (laneWidth <= 0f)
        {
            return;
        }

        int laneCount = Mathf.Min(config.LaneCount, laneRenderers != null ? laneRenderers.Length : 0);
        if (laneCount <= 0)
        {
            return;
        }

        float cameraHeight = cam.orthographic ? cam.orthographicSize * 2f : 0f;

        for (int i = 0; i < laneCount; i++)
        {
            SpriteRenderer laneRenderer = laneRenderers[i];
            if (laneRenderer == null)
            {
                continue;
            }

            if (!config.TryGetLaneX(i, out float laneX))
            {
                continue;
            }

            Vector3 lanePosition = laneRenderer.transform.position;
            laneRenderer.transform.position = new Vector3(laneX, lanePosition.y, lanePosition.z);

            ApplyRendererScale(i, laneRenderer, laneWidth, cameraHeight);
        }
    }

    private void ApplyRendererScale(int laneIndex, SpriteRenderer renderer, float laneWidth, float cameraHeight)
    {
        if (renderer.sprite == null)
        {
            return;
        }

        if (baseScales == null || laneIndex >= baseScales.Length)
        {
            CacheBaseScales();
        }

        Vector3 baseScale = baseScales != null && laneIndex < baseScales.Length ? baseScales[laneIndex] : renderer.transform.localScale;

        float spriteWidth = renderer.sprite.bounds.size.x;
        float spriteHeight = renderer.sprite.bounds.size.y;
        if (spriteWidth <= 0f || spriteHeight <= 0f)
        {
            return;
        }

        float scaleX = laneWidth / spriteWidth;
        float scaleY = baseScale.y;

        if (stretchToCameraHeight && cameraHeight > 0f)
        {
            scaleY = cameraHeight / spriteHeight;
        }

        renderer.transform.localScale = new Vector3(scaleX, scaleY, baseScale.z);
    }

    private GameConfig ResolveConfig()
    {
        if (gameConfig != null)
        {
            return gameConfig;
        }

        if (GameManager.Instance != null)
        {
            return GameManager.Instance.Config;
        }

        return null;
    }

    private Camera ResolveCamera()
    {
        if (targetCamera != null)
        {
            return targetCamera;
        }

        if (Camera.main != null)
        {
            return Camera.main;
        }

        return null;
    }

    private void CacheBaseScales()
    {
        if (laneRenderers == null)
        {
            baseScales = null;
            return;
        }

        baseScales = new Vector3[laneRenderers.Length];
        for (int i = 0; i < laneRenderers.Length; i++)
        {
            SpriteRenderer laneRenderer = laneRenderers[i];
            baseScales[i] = laneRenderer != null ? laneRenderer.transform.localScale : Vector3.one;
        }
    }

    private void CacheScreenSize()
    {
        cachedScreenWidth = Screen.width;
        cachedScreenHeight = Screen.height;
    }
}
