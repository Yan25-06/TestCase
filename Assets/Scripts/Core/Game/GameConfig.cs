using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "PianoTiles/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Lane Layout")]
    [SerializeField] private float[] laneXPositions = new float[] { -1.5f, -0.5f, 0.5f, 1.5f };

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

    public float[] LaneXPositions => laneXPositions;
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
        if (laneXPositions == null || laneIndex < 0 || laneIndex >= laneXPositions.Length)
        {
            return false;
        }

        x = laneXPositions[laneIndex];
        return true;
    }
}
