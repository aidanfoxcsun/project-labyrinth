using System.Collections.Generic;
using UnityEngine;

public class ItemPoolManager : MonoBehaviour
{
    public static ItemPoolManager Instance;

    public List<ItemData> standardPool = new List<ItemData>();

    void Awake()
    {
        Instance = this;
    }
}
