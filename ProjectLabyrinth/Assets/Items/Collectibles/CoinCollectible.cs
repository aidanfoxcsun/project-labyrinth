using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    public int coinValue = 1; // default penny

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.coins += coinValue;
                // Optionally, play a sound effect or animation here
                Destroy(gameObject); // Remove the coin from the scene
                Debug.Log("Collected a coin! Total coins: " + playerStats.coins);
            }
        }
    }
}
