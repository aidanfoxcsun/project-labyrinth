using UnityEngine;

// Sits on the anvil GameObject and notifies the UpgradeRoomController
// when the player touches it. Kept separate so UpgradeRoomController
// does not need to be on the same object as the collider.
[RequireComponent(typeof(Collider2D))]
public class AnvilInteractable : MonoBehaviour
{
    // Set by UpgradeRoomController after spawning this object
    public UpgradeRoomController controller;

    void Awake()
    {
        // Make sure the collider is a trigger
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (controller == null) return;

        controller.OnPlayerTouchAnvil(other);
    }
}
