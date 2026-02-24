using UnityEngine;

public class PortalToNextFloor : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var floor = FindFirstObjectByType<FloorManager>();
        if (floor == null)
        {
            Debug.LogError("[PortalToNextFloor] No FloorManager in scene.");
            return;
        }

        floor.AdvanceFloor();
        Destroy(gameObject);
    }
}