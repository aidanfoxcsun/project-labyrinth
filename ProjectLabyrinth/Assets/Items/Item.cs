using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Item : MonoBehaviour
{
    private ItemData data;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChooseItemFromPool();
        GetComponent<SpriteRenderer>().sprite = data.icon;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.transform.GetComponent<PlayerStats>();
            data.Apply(stats);
            collision.transform.GetComponent<PlayerMovement>().UpdateStats();
            Destroy(this.gameObject);
        }
    }

    private void ChooseItemFromPool()
    {
        data = ItemPools.ChooseRandom(ItemPoolManager.Instance.standardPool);
    }
}
