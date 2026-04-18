using System;
using UnityEngine;

[Serializable]
public class PoolConfig
{
    [SerializeField] private string key;
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 8;
    [SerializeField] private int maxSize = 64;
    [SerializeField] private bool allowExpand = true;

    public string Key => key;
    public GameObject Prefab => prefab;
    public int InitialSize => initialSize;
    public int MaxSize => maxSize;
    public bool AllowExpand => allowExpand;
}
