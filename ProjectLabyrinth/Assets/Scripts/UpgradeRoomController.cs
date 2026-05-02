using UnityEngine;
using TMPro;

// Controls the Upgrade Room layout.
// Spawns 3 random shop items horizontally in the center of the room
// and an anvil above them. The anvil is a one-time-use damage buff
// that either adds flat damage or a damage scaling percentage.
public class UpgradeRoomController : MonoBehaviour
{
    // --- Shop Items ---

    [Header("Shop Items")]
    // Prefab with a SpriteRenderer, Collider2D, and ShopItem component
    public GameObject shopItemPrefab;
    public int itemCost = 10;

    // Horizontal spacing between the 3 items
    public float itemSpacing = 2f;

    // --- Anvil ---

    [Header("Anvil")]
    // Prefab with a SpriteRenderer and Collider2D for the anvil
    public GameObject anvilPrefab;

    // How far above the items the anvil sits
    public float anvilOffsetY = 2f;

    // Flat damage added by the anvil buff (used when buff type is flat)
    public float anvilFlatDamage = 1f;

    // Damage scaling added by the anvil buff (used when buff type is percent)
    // 0.10 = +10%
    public float anvilDamageScaling = 0.10f;

    // --- Runtime ---

    private bool anvilUsed = false;
    private GameObject anvilInstance;

    void Start()
    {
        SpawnShopItems();
        SpawnAnvil();
    }

    // Picks 3 unique random items from the pool and places them side by side
    void SpawnShopItems()
    {
        if (shopItemPrefab == null)
        {
            Debug.LogWarning("[UpgradeRoomController] shopItemPrefab is not assigned.");
            return;
        }

        if (ItemPoolManager.Instance == null || ItemPoolManager.Instance.ingamePool.Count == 0)
        {
            Debug.LogWarning("[UpgradeRoomController] ItemPoolManager has no items in the pool.");
            return;
        }

        // Pick 3 items, avoiding duplicates where possible
        var pool = ItemPoolManager.Instance.ingamePool;
        var chosen = new System.Collections.Generic.List<ItemData>();

        int attempts = 0;
        while (chosen.Count < 3 && attempts < 50)
        {
            attempts++;
            ItemData candidate = ItemPools.ChooseRandom(pool);
            if (!chosen.Contains(candidate))
                chosen.Add(candidate);
        }

        // Positions: center, left, right
        float[] offsets = { -itemSpacing, 0f, itemSpacing };

        for (int i = 0; i < chosen.Count; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(offsets[i], 0f, 0f);
            GameObject obj = Instantiate(shopItemPrefab, spawnPos, Quaternion.identity, transform);

            ShopItem shopItem = obj.GetComponent<ShopItem>();
            if (shopItem != null)
            {
                shopItem.data = chosen[i];
                shopItem.cost = itemCost;
                shopItem.Initialize();
            }
        }
    }

    // Places the anvil above the center item
    void SpawnAnvil()
    {
        if (anvilPrefab == null)
        {
            Debug.LogWarning("[UpgradeRoomController] anvilPrefab is not assigned.");
            return;
        }

        Vector3 anvilPos = transform.position + new Vector3(0f, anvilOffsetY, 0f);
        anvilInstance = Instantiate(anvilPrefab, anvilPos, Quaternion.identity, transform);

        // Add the trigger component at runtime so the prefab stays simple
        AnvilInteractable interactable = anvilInstance.AddComponent<AnvilInteractable>();
        interactable.controller = this;
    }

    // Called by AnvilInteractable when the player touches the anvil
    public void OnPlayerTouchAnvil(Collider2D playerCollider)
    {
        if (anvilUsed) return;

        PlayerStats stats = playerCollider.GetComponent<PlayerStats>();
        if (stats == null) return;

        if (!stats.SpendCoins(5))
        {
            Debug.Log("[UpgradeRoomController] Player cannot afford the anvil buff. Player coins: " + stats.coins);
            return;
        }

        // Randomly pick flat or percent buff
        bool useFlatBuff = Random.value < 0.5f;

        if (useFlatBuff)
        {
            stats.flatDamage += anvilFlatDamage;
            Debug.Log("[UpgradeRoomController] Anvil applied flat damage +" + anvilFlatDamage);
        }
        else
        {
            stats.damageScaling += anvilDamageScaling;
            Debug.Log("[UpgradeRoomController] Anvil applied damage scaling +" + (anvilDamageScaling * 100f) + "%");
        }

        stats.UpdateStats();

        anvilUsed = true;

        // Grey out the anvil to signal it has been used
        SpriteRenderer sr = anvilInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(0.4f, 0.4f, 0.4f, 1f);
    }
}
