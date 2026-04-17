using UnityEngine;
using TMPro;

// A purchasable item in the UpgradeRoom.
// Displays the item icon and a price tag. On player contact,
// spends coins and applies the item effect if the player can afford it.
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ShopItem : MonoBehaviour
{
    [Header("Item")]
    public ItemData data;
    public int cost = 10;

    [Header("Price Tag")]
    // World-space TMP text sitting below the item sprite
    public TMP_Text priceText;

    private bool purchased = false;

    // Called by UpgradeRoomController after assigning data and cost
    public void Initialize()
    {
        if (data == null) return;

        // Display the item icon
        GetComponent<SpriteRenderer>().sprite = data.icon;

        // Display the price
        if (priceText != null)
            priceText.text = cost + " coins";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (purchased) return;
        if (!other.CompareTag("Player")) return;

        HUDController hud = FindObjectOfType<HUDController>();
        if (hud == null) return;

        // Check if the player can afford it
        if (!hud.SpendCoins(cost))
        {
            Debug.Log("[ShopItem] Not enough coins to purchase " + data.itemName);
            return;
        }

        // Apply the item effect to the player
        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats != null)
            data.Apply(stats);

        purchased = true;

        // Hide the item after purchase
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        if (priceText != null)
            priceText.gameObject.SetActive(false);
    }
}
