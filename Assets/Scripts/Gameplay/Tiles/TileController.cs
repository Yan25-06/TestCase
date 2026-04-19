using UnityEngine;

public class TileController : MonoBehaviour, IPoolable
{
    [Header("Tile Settings")]
    [SerializeField] private TileType tileType = TileType.ShortTile;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private Sprite deadHeadSprite;
    [SerializeField] private SpriteRenderer sizeReferenceRenderer;

    private bool isTapped;
    private bool isActive;
    private int laneIndex;
    private string poolKey;
    private Vector3 baseLocalScale;

    public int LaneIndex => laneIndex;
    public TileType TileType => tileType;

    public void Configure(int newLaneIndex, string newPoolKey, TileType newTileType)
    {
        laneIndex = newLaneIndex;
        poolKey = newPoolKey;
        tileType = newTileType;

        FitToLaneWidth();
    }

    private void Awake()
    {
        baseLocalScale = transform.localScale;

        // A StartTile placed directly in scene should still be clickable before pooling starts.
        if (tileType == TileType.StartTile && string.IsNullOrWhiteSpace(poolKey))
        {
            isActive = true;
        }
    }

    private void Update()
    {
        GameManager manager = GameManager.Instance;
        if (!isActive || manager == null || manager.CurrentState != GameState.Playing)
        {
            return;
        }

        transform.Translate(TileMovement.ComputeStep(manager.CurrentScrollSpeed));

        if (transform.position.y < manager.Config.MissY)
        {
            if (tileType != TileType.StartTile && !isTapped)
            {
                manager.TriggerGameOver();
            }

            RequestDespawn();
        }
    }

    private void OnMouseDown()
    {
        if (isTapped)
        {
            return;
        }

        GameManager manager = GameManager.Instance;

        if (tileType == TileType.StartTile)
        {
            isTapped = true;

            if (headRenderer != null && deadHeadSprite != null)
            {
                headRenderer.sprite = deadHeadSprite;
            }

            if (manager != null && manager.CurrentState == GameState.Idle)
            {
                manager.StartGame();
            }

            if (!string.IsNullOrWhiteSpace(poolKey))
            {
                RequestDespawn();
            }

            return;
        }

        if (!isActive)
        {
            return;
        }

        isTapped = true;
        if (headRenderer != null && deadHeadSprite != null)
        {
            headRenderer.sprite = deadHeadSprite;
        }

        if (manager != null)
        {
            manager.AddScore(1);
        }

        RequestDespawn();
    }

    public void OnSpawned()
    {
        isActive = true;
        isTapped = false;
    }

    public void OnDespawned()
    {
        isActive = false;
        isTapped = false;
    }

    private void RequestDespawn()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null || manager.Pool == null || string.IsNullOrWhiteSpace(poolKey))
        {
            gameObject.SetActive(false);
            return;
        }

        if (!manager.Pool.Despawn(poolKey, gameObject))
        {
            manager.Pool.Despawn(gameObject);
        }
    }

    private void FitToLaneWidth()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null || manager.Config == null)
        {
            return;
        }

        float laneWidth = manager.Config.LaneWidth;
        if (laneWidth <= 0f)
        {
            return;
        }

        SpriteRenderer referenceRenderer = sizeReferenceRenderer != null ? sizeReferenceRenderer : GetComponentInChildren<SpriteRenderer>();
        if (referenceRenderer == null || referenceRenderer.sprite == null)
        {
            return;
        }

        float referenceWidthAtUnitScale = referenceRenderer.sprite.bounds.size.x;
        if (referenceWidthAtUnitScale <= 0f)
        {
            return;
        }

        float targetScaleX = laneWidth / referenceWidthAtUnitScale;
        transform.localScale = new Vector3(targetScaleX, baseLocalScale.y, baseLocalScale.z);
    }
}
