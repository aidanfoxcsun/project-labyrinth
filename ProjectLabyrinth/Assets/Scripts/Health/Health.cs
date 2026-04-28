using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Health : MonoBehaviour
{
    public float maxHP = 10;
    public float hitPoints = 10;

    public event System.Action OnDeath;
    public event System.Action OnHit;

    public bool isPlayer = false;

    [Header("Coin Drop (enemies only)")]
    public GameObject coinPrefab;
    public int coinDropAmount = 1;

    
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

        if (!isPlayer && coinPrefab != null)
        {
            GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
            CoinPickup pickup = coin.GetComponent<CoinPickup>();
            if (pickup != null)
                pickup.value = coinDropAmount;
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