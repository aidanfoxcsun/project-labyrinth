using UnityEngine;
using System.Collections;

// Coin that enemies drop on death.
// On player contact, adds coins to the HUD and destroys itself.
public class CoinPickup : MonoBehaviour
{
    [Header("Coin Settings")]
    public int value = 1;
    public AudioSource pickupSound;

    private void Start()
    {
        pickupSound.volume = AudioController.Instance.sfxVolume;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Find the HUD and add the coin value
        PlayerStats stats = other.gameObject.GetComponent<PlayerStats>();

        if (stats != null)
            stats.coins += value;

        GetComponent<SpriteRenderer>().enabled = false; // Hide the coin sprite immediately
        GetComponent<Collider2D>().enabled = false; // Disable the collider to prevent further triggers
        pickupSound?.Play(); // Play pickup sound if assigned
        StartCoroutine(waitToDestroy());
    }

    private IEnumerator waitToDestroy()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

}
