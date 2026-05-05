using UnityEngine;
using System.Collections;

public class BombActive : MonoBehaviour
{
    public float explosionRadius = 2f;
    public AudioSource explosionSound;
    public float damage = 5f;
    public float fuseTime = 2f;
    public LayerMask damageableLayers;

    public ParticleSystem explosionEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ExplodeAfterDelay(fuseTime));
    }

    private IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        explosionEffect?.Play();
        explosionSound?.Play();
        Explode();
    }

    private void Explode()
    {
        // Find all colliders in the explosion radius that are on the damageable layers
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        foreach (Collider2D hit in hitColliders)
        {
            // Check if the collider has a Health component and apply damage
            Health health = hit.GetComponent<Health>();
            if (health != null && !hit.CompareTag("Player"))
            {
                health.RecieveDamage(damage); 
            }
            else if (health != null && hit.CompareTag("Player"))
            {
                health.RecieveDamage(1f); 
            }
            else if (hit.CompareTag("Rock"))
            {
                Destroy(hit.gameObject);
            }
        }
        
        GetComponent<SpriteRenderer>().enabled = false; // Hide the bomb sprite after explosion
        StartCoroutine(DestroyAfterEffect());
    }

    private IEnumerator DestroyAfterEffect()
    {
        // Wait for the explosion effect to finish before destroying the bomb object
        if (explosionEffect != null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        Destroy(gameObject);
    }
}
