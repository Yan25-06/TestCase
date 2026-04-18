using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly PoolConfig config;
    private readonly Transform poolRoot;
    private readonly Queue<GameObject> inactiveQueue = new Queue<GameObject>();
    private readonly HashSet<GameObject> activeSet = new HashSet<GameObject>();

    private int totalCreated;

    public ObjectPool(PoolConfig config, Transform rootParent)
    {
        this.config = config;
        poolRoot = new GameObject($"Pool_{config.Key}").transform;
        poolRoot.SetParent(rootParent, false);
    }

    public int ActiveCount => activeSet.Count;

    public void Prewarm()
    {
        for (int i = 0; i < config.InitialSize; i++)
        {
            GameObject created = CreateInstance();
            if (created == null)
            {
                return;
            }

            DeactivateAndStore(created);
        }
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject instance = null;

        if (inactiveQueue.Count > 0)
        {
            instance = inactiveQueue.Dequeue();
        }
        else if (CanCreateMore())
        {
            instance = CreateInstance();
        }

        if (instance == null)
        {
            return null;
        }

        activeSet.Add(instance);

        instance.transform.SetParent(parent, false);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);

        NotifySpawned(instance);
        return instance;
    }

    public bool Despawn(GameObject instance)
    {
        if (instance == null || !activeSet.Contains(instance))
        {
            return false;
        }

        activeSet.Remove(instance);
        NotifyDespawned(instance);
        DeactivateAndStore(instance);
        return true;
    }

    public void DespawnAllActive()
    {
        if (activeSet.Count == 0)
        {
            return;
        }

        List<GameObject> toDespawn = new List<GameObject>(activeSet);
        for (int i = 0; i < toDespawn.Count; i++)
        {
            Despawn(toDespawn[i]);
        }
    }

    private bool CanCreateMore()
    {
        if (config.AllowExpand)
        {
            return config.MaxSize <= 0 || totalCreated < config.MaxSize;
        }

        return false;
    }

    private GameObject CreateInstance()
    {
        if (config.Prefab == null)
        {
            return null;
        }

        GameObject instance = Object.Instantiate(config.Prefab, poolRoot);
        instance.SetActive(false);
        totalCreated++;
        return instance;
    }

    private void DeactivateAndStore(GameObject instance)
    {
        instance.transform.SetParent(poolRoot, false);
        instance.SetActive(false);
        inactiveQueue.Enqueue(instance);
    }

    private static void NotifySpawned(GameObject instance)
    {
        IPoolable[] listeners = instance.GetComponents<IPoolable>();
        for (int i = 0; i < listeners.Length; i++)
        {
            listeners[i].OnSpawned();
        }
    }

    private static void NotifyDespawned(GameObject instance)
    {
        IPoolable[] listeners = instance.GetComponents<IPoolable>();
        for (int i = 0; i < listeners.Length; i++)
        {
            listeners[i].OnDespawned();
        }
    }
}
