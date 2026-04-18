public readonly struct SpawnSequence
{
    public SpawnSequence(int spawnIndex, int laneIndex, float spawnTime)
    {
        SpawnIndex = spawnIndex;
        LaneIndex = laneIndex;
        SpawnTime = spawnTime;
    }

    public int SpawnIndex { get; }
    public int LaneIndex { get; }
    public float SpawnTime { get; }
}
