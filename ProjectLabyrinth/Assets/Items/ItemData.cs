using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public float weight; // How likely to encounter this item

    [SerializeReference]
    public List<ItemEffect> effects = new List<ItemEffect>();

    public void Apply(PlayerStats stats)
    {
        Debug.Log($"Item Acquired: {itemName}\n\"{description}\"");
        foreach (var effect in effects)
        {
            effect.Apply(stats);
            Debug.Log(effect.DebugEffects());
        }
    }
}
