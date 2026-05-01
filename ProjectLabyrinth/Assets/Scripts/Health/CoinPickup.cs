using UnityEngine;

// Coin that enemies drop on death.
// On player contact, adds coins to the HUD and destroys itself.
public class CoinPickup : MonoBehaviour
{
    [Header("Coin Settings")]
    public int value = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Find the HUD and add the coin value
        PlayerStats stats = other.gameObject.GetComponent<PlayerStats>();

        if (stats != null)
            stats.coins += value;

        Destroy(gameObject);
    }
}
