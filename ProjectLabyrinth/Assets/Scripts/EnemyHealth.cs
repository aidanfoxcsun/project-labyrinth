using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int hp = 3;

    [Header("Coin Drop")]
    public GameObject coinPrefab;
    public int coinDropAmount = 1;

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            DropCoin();
            Destroy(gameObject);
        }
    }

    // Spawns a coin pickup at the enemy's position on death
    void DropCoin()
    {
        if (coinPrefab == null) return;

        GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);

        // Set the coin value if the prefab has a CoinPickup component
        CoinPickup pickup = coin.GetComponent<CoinPickup>();
        if (pickup != null)
            pickup.value = coinDropAmount;
    }
}