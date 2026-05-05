using UnityEngine;
using System.Collections;

public class BombCollectible : MonoBehaviour
{
    public int bombValue = 1; // default bomb
    public AudioSource pickupSound;

    private void Start()
    {
        pickupSound.volume = AudioController.Instance.sfxVolume;    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.bombs += bombValue;
                // Optionally, play a sound effect or animation here

                GetComponent<SpriteRenderer>().enabled = false; // Hide the coin sprite immediately
                GetComponent<Collider2D>().enabled = false; // Disable the collider to prevent further triggers
                pickupSound?.Play(); // Play pickup sound if assigned
                StartCoroutine(waitToDestroy());
                Debug.Log("Collected a bomb! Total bombs: " + playerStats.bombs);
            }
        }
    }

    private IEnumerator waitToDestroy()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
