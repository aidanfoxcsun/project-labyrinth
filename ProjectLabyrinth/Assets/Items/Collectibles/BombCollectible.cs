using UnityEngine;

public class BombCollectible : MonoBehaviour
{
    public int bombValue = 1; // default bomb

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.bombs += bombValue;
                // Optionally, play a sound effect or animation here
                Destroy(gameObject); // Remove the coin from the scene
                Debug.Log("Collected a bomb! Total bombs: " + playerStats.bombs);
            }
        }
    }
}
