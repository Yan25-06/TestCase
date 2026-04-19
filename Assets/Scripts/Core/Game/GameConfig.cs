using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "PianoTiles/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Lane Layout")]
    [SerializeField] private int laneCount = 4;
    [SerializeField] private float horizontalPadding = 0f;
    [SerializeField] private float[] fallbackLaneXPositions = new float[] { -1.5f, -0.5f, 0.5f, 1.5f };

    [Header("Scroll")]
    [SerializeField] private float baseScrollSpeed = 4f;
    [SerializeField] private float speedIncreasePerScore = 0.03f;

    [Header("Spawn")]
    [SerializeField] private float spawnInterval = 0.35f;
    [SerializeField] private float spawnY = 7f;
    [SerializeField] private float missY = -7f;

    [Header("Hit Window")]
    [SerializeField] private float hitWindowTop = -3.1f;
    [SerializeField] private float hitWindowBottom = -4.1f;

    private float[] runtimeLaneXPositions;
    private float runtimeLaneWidth;

    public int LaneCount => Mathf.Max(1, laneCount);
    public float[] LaneXPositions => runtimeLaneXPositions != null && runtimeLaneXPositions.Length > 0 ? runtimeLaneXPositions : fallbackLaneXPositions;
    public float LaneWidth => runtimeLaneWidth;
    public float BaseScrollSpeed => baseScrollSpeed;
    public float SpeedIncreasePerScore => speedIncreasePerScore;
    public float SpawnInterval => spawnInterval;
    public float SpawnY => spawnY;
    public float MissY => missY;
    public float HitWindowTop => hitWindowTop;
    public float HitWindowBottom => hitWindowBottom;

    public bool TryGetLaneX(int laneIndex, out float x)
    {
        x = 0f;
        float[] lanePositions = LaneXPositions;
        if (lanePositions == null || laneIndex < 0 || laneIndex >= lanePositions.Length)
        {
            return false;
        }

        x = lanePositions[laneIndex];
        return true;
    }

    public void RecalculateLaneLayout(Camera targetCamera)
    {
        if (targetCamera == null || !targetCamera.orthographic)
        {
            runtimeLaneXPositions = fallbackLaneXPositions;
            runtimeLaneWidth = 0f;
            return;
        }

        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;

        float totalWidth = Mathf.Max(0.1f, (halfWidth * 2f) - (horizontalPadding * 2f));
        int count = LaneCount;
        runtimeLaneWidth = totalWidth / count;
        runtimeLaneXPositions = new float[count];

        float leftEdge = -halfWidth + horizontalPadding;
        for (int i = 0; i < count; i++)
        {
            runtimeLaneXPositions[i] = leftEdge + runtimeLaneWidth * (i + 0.5f);
        }
    }
}
