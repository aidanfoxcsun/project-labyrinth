using UnityEngine;
using System.Collections.Generic;

public static class ItemPools
{
    public static ItemData ChooseRandom(List<ItemData> pool)
    {
        float totalWeight = 0.0f;

        foreach(var item in pool)
        {
            totalWeight += item.weight;
        }

        float randomValue = Random.value * totalWeight;

        float cumulative = 0.0f;
        foreach(var item in pool)
        {
            cumulative += item.weight;
            if (randomValue <= cumulative)
            {
                return item;
            }
        }

        // fallback if the above doesn't work for whatever reason.
        return pool[pool.Count - 1];
    }
}
