using System.Collections.Generic;
using UnityEngine;

public class ItemPoolManager : MonoBehaviour
{
    public static ItemPoolManager Instance;

    [SerializeField] private List<ItemData> standardPool = new List<ItemData>();

    public List<ItemData> ingamePool;

    void Awake()
    {
        Instance = this;
        ingamePool = standardPool;
    }

    public void removeFromPool(ItemData item)
    {
        if (ingamePool.Count == 0)
        {
            ingamePool = standardPool;
        }
        ingamePool.Remove(item);
    }
}
