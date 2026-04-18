using UnityEngine;

public class TileController : MonoBehaviour, IPoolable
{
    [Header("Tile Settings")]
    [SerializeField] private TileType tileType = TileType.ShortTile;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private Sprite deadHeadSprite;

    private bool isTapped;
    private bool isActive;
    private int laneIndex;
    private string poolKey;

    public int LaneIndex => laneIndex;
    public TileType TileType => tileType;

    public void Configure(int newLaneIndex, string newPoolKey, TileType newTileType)
    {
        laneIndex = newLaneIndex;
        poolKey = newPoolKey;
        tileType = newTileType;
    }

    private void Update()
    {
        GameManager manager = GameManager.Instance;
        if (!isActive || manager == null || manager.CurrentState != GameState.Playing)
        {
            return;
        }

        transform.Translate(TileMovement.ComputeStep(manager.CurrentScrollSpeed));

        if (transform.position.y < manager.Config.MissY && !isTapped)
        {
            manager.TriggerGameOver();
            RequestDespawn();
        }
    }

    private void OnMouseDown()
    {
        if (!isActive || isTapped)
        {
            return;
        }

        isTapped = true;
        if (headRenderer != null && deadHeadSprite != null)
        {
            headRenderer.sprite = deadHeadSprite;
        }

        GameManager manager = GameManager.Instance;
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
}
