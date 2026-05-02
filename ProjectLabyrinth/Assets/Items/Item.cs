using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Item : MonoBehaviour
{
    private ItemData data;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChooseItemFromPool();
        ItemPoolManager.Instance.removeFromPool(data);
        GetComponent<SpriteRenderer>().sprite = data.icon;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerStats stats = collision.GetComponent<PlayerStats>();
        PlayerMovement movement = collision.GetComponent<PlayerMovement>();

        if (stats == null) return;

        // 1. Apply stat changes first
        data.Apply(stats);

        // 2. Then sync the Health/Movement components with the new stats
        movement?.ApplyStats();

        // 3. Then tell the HUD to read the new values — delta is now correct
        stats.CollectItem(data);

        Destroy(gameObject);
    }

    private void ChooseItemFromPool()
    {
        data = ItemPools.ChooseRandom(ItemPoolManager.Instance.ingamePool);
    }
}
