using UnityEngine;

public static class TileMovement
{
    public static Vector3 ComputeStep(float speed)
    {
        return Vector3.down * speed * Time.deltaTime;
    }
}
