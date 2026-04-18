using System;

public class SpawnPatternProvider
{
    private readonly int laneCount;
    private readonly int maxConsecutiveSameLane;

    private readonly Random random;
    private int previousLane = -1;
    private int consecutiveCount;

    public SpawnPatternProvider(int laneCount, int maxConsecutiveSameLane, int seed)
    {
        this.laneCount = laneCount;
        this.maxConsecutiveSameLane = Math.Max(1, maxConsecutiveSameLane);
        random = seed >= 0 ? new Random(seed) : new Random();
    }

    public void Reset()
    {
        previousLane = -1;
        consecutiveCount = 0;
    }

    public int GetNextLane()
    {
        if (laneCount <= 1)
        {
            return 0;
        }

        int candidate = random.Next(0, laneCount);

        if (previousLane >= 0 && consecutiveCount >= maxConsecutiveSameLane)
        {
            int attempts = 0;
            while (candidate == previousLane && attempts < 8)
            {
                candidate = random.Next(0, laneCount);
                attempts++;
            }

            if (candidate == previousLane)
            {
                candidate = (previousLane + 1) % laneCount;
            }
        }

        if (candidate == previousLane)
        {
            consecutiveCount++;
        }
        else
        {
            previousLane = candidate;
            consecutiveCount = 1;
        }

        return candidate;
    }
}
