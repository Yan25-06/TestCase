using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] private List<PoolConfig> poolConfigs = new List<PoolConfig>();

    private readonly Dictionary<string, ObjectPool> poolsByKey = new Dictionary<string, ObjectPool>();
    private readonly Dictionary<GameObject, string> activeInstanceToPoolKey = new Dictionary<GameObject, string>();

    private bool isInitialized;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    public void InitializeIfNeeded()
    {
        if (isInitialized)
        {
            return;
        }

        poolsByKey.Clear();

        for (int i = 0; i < poolConfigs.Count; i++)
        {
            PoolConfig config = poolConfigs[i];
            if (config == null || string.IsNullOrWhiteSpace(config.Key) || config.Prefab == null)
            {
                continue;
            }

            if (poolsByKey.ContainsKey(config.Key))
            {
                Debug.LogWarning($"Duplicate pool key '{config.Key}' ignored.");
                continue;
            }

            poolsByKey.Add(config.Key, new ObjectPool(config, transform));
        }

        isInitialized = true;
    }

    public void PrewarmAll()
    {
        InitializeIfNeeded();

        foreach (KeyValuePair<string, ObjectPool> pair in poolsByKey)
        {
            pair.Value.Prewarm();
        }
    }

    public GameObject Spawn(string poolKey, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        InitializeIfNeeded();

        if (!poolsByKey.TryGetValue(poolKey, out ObjectPool pool))
        {
            Debug.LogWarning($"Pool key '{poolKey}' not found.");
            return null;
        }

        GameObject instance = pool.Spawn(position, rotation, parent);
        if (instance != null)
        {
            activeInstanceToPoolKey[instance] = poolKey;
        }

        return instance;
    }

    public bool Despawn(string poolKey, GameObject instance)
    {
        InitializeIfNeeded();

        if (instance == null)
        {
            return false;
        }

        if (!poolsByKey.TryGetValue(poolKey, out ObjectPool pool))
        {
            return false;
        }

        bool success = pool.Despawn(instance);
        if (success)
        {
            activeInstanceToPoolKey.Remove(instance);
        }

        return success;
    }

    public bool Despawn(GameObject instance)
    {
        InitializeIfNeeded();

        if (instance == null)
        {
            return false;
        }

        if (!activeInstanceToPoolKey.TryGetValue(instance, out string poolKey))
        {
            return false;
        }

        return Despawn(poolKey, instance);
    }

    public void DespawnAllActive()
    {
        InitializeIfNeeded();

        foreach (KeyValuePair<string, ObjectPool> pair in poolsByKey)
        {
            pair.Value.DespawnAllActive();
        }

        activeInstanceToPoolKey.Clear();
    }
}
