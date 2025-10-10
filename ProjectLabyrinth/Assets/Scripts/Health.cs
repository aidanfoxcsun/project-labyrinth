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
            return true;
        }

        return false;
    }

    // Triggers the death behavior(s) of the entity
    private void Die()
    {
        OnDeath?.Invoke();
    }

    // Reacting to collisions with damagers
    private void OnCollisionEnter2D(Collision2D collision)
    {
        IDamager damager = collision.gameObject.GetComponent<IDamager>();
        if (damager != null)
        {
            RecieveDamage(damager.DamageAmount);
        }
    }

}
