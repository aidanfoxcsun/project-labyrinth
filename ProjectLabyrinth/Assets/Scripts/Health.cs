using UnityEngine;

// Health Component for general entities

[RequireComponent (typeof(Collider2D))]

public class Health : MonoBehaviour
{
    public float maxHP = 10;
    public float hitPoints = 10;

    public event System.Action OnDeath; // Subscribe to this event to enable death behavior
    public event System.Action OnHit;   // Subscribe to this event to enable hit behavior
    // Expand for healing behaviors

    public bool isPlayer = false;

    [Header("Coin Drop (enemies only)")]
    public GameObject coinPrefab;
    public int coinDropAmount = 1;

    public void SetIsPlayer(bool value)
    {
        isPlayer = value;
    }

    public void Heal(float amount)
    {
        hitPoints += amount;
        if (hitPoints > maxHP)
        {
            hitPoints = maxHP;
        }
    }

    public void FullHeal()
    {
        hitPoints = maxHP;
    }

    // Method to inflict damage on this entity. Returns true if the entity is "killed"
    public bool RecieveDamage(float damage) 
    {
        OnHit?.Invoke();
        hitPoints -= damage;

        if (hitPoints <= 0)
        {
            Die();
            Debug.Log(this.name + " died!");
            return true;
        }

        return false;
    }

    // Triggers the death behavior(s) of the entity
    private void Die()
    {
        OnDeath?.Invoke();

        // Drop a coin if this is an enemy and a coin prefab is assigned
        if (!isPlayer && coinPrefab != null)
        {
            GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
            CoinPickup pickup = coin.GetComponent<CoinPickup>();
            if (pickup != null)
                pickup.value = coinDropAmount;
        }
    }

    // Reacting to collisions with damagers
    private void OnCollisionEnter2D(Collision2D collision)
    {
        IDamager damager = collision.gameObject.GetComponent<IDamager>();
        if (damager != null)
        {
            Debug.Log("Damager hit!");
            if((isPlayer && damager.PlayerSourced) ||  (!isPlayer && !damager.PlayerSourced)) { return; }
            RecieveDamage(damager.DamageAmount);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamager damager = collision.gameObject.GetComponent<IDamager>();
        if (damager != null)
        {
            Debug.Log("Damager hit!");
            // if((isPlayer && damager.PlayerSourced) ||  (!isPlayer && !damager.PlayerSourced)) { return; }
            RecieveDamage(damager.DamageAmount);
        }
    }

}
