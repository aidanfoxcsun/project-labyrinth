using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class Health : MonoBehaviour
{
    public float maxHP = 10;
    public float hitPoints = 10;

    public event System.Action OnDeath;
    public event System.Action OnHit;

    public bool isPlayer = false;

    [Header("Collectable Drops (enemies only)")]
    public float dropChance = 0.75f; // Chance to drop collectables on death
    public List<GameObject> droppables;
    public int dropAmount = 1;

    
    private PlayerStats playerStats;

    private void Awake()
    {
        if (isPlayer)
        {
            playerStats = GetComponent<PlayerStats>();

            if (playerStats != null)
            {
                // Health is the source of truth for HP values
                maxHP = playerStats.maxHP;
                hitPoints = playerStats.currentHP;
            }
        }
    }
    

    public void SetIsPlayer(bool value)
    {
        isPlayer = value;
    }

    public void Heal(float amount)
    {
        hitPoints = Mathf.Min(hitPoints + amount, maxHP);
        SyncToPlayerStats();
    }

    public void FullHeal()
    {
        hitPoints = maxHP;
        SyncToPlayerStats();
    }

    public bool RecieveDamage(float damage)
    {
        OnHit?.Invoke();
        hitPoints -= damage;
        SyncToPlayerStats();

        if (hitPoints <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    // Pushes Health's hitPoints into PlayerStats/HUD after any change
    private void SyncToPlayerStats()
    {
        if (playerStats != null)
            playerStats.currentHP = hitPoints;
    }

    private void Die()
    {
        OnDeath?.Invoke();

        if (!isPlayer && droppables.Count > 0)
        {
            if(UnityEngine.Random.value <= dropChance)
            {
                Instantiate(droppables[Random.Range(0, droppables.Count)], transform.position, Quaternion.identity);    
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        IDamager damager = collision.gameObject.GetComponent<IDamager>();
        if (damager == null) return;

        if ((isPlayer && damager.PlayerSourced) || (!isPlayer && !damager.PlayerSourced)) return;

        RecieveDamage(damager.DamageAmount);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamager damager = collision.gameObject.GetComponent<IDamager>();
        if (damager == null) return;

        RecieveDamage(damager.DamageAmount);
    }
}