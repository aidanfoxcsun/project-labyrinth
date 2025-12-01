using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    public Vector2Int direction;
    public Vector2Int parentGrid;
    private DungeonGenerator generator;

    // === Added for room locking ===
    public bool locked = false;

    public void SetLocked(bool state)
    {
        locked = state;
        // (Optional: change sprite / color later)
    }

public void SetDoorActive(bool active)
{
    var col = GetComponent<Collider2D>();
    if (col != null)
        col.enabled = active;
}

    void Start() => generator = FindObjectOfType<DungeonGenerator>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (locked) return;   // <--- prevents teleporting when locked
        if (!other.CompareTag("Player")) return;

        Vector2Int nextGrid = parentGrid + direction;

        if (generator != null && generator.HasRoom(nextGrid))
        {
            GameObject targetRoom = generator.GetRoom(nextGrid);

            Vector2Int oppositeDir = -direction;
            string oppositeDoorName = oppositeDir == Vector2Int.up ? "Door_N" :
                                      oppositeDir == Vector2Int.down ? "Door_S" :
                                      oppositeDir == Vector2Int.right ? "Door_E" :
                                      "Door_W";

            Transform entry = targetRoom.transform.Find(oppositeDoorName + "/EntryPoint");

            if (entry != null)
            {
                StartCoroutine(TeleportPlayer(other.gameObject, entry.position));
            }
        }
    }

    private IEnumerator TeleportPlayer(GameObject player, Vector3 targetPos)
    {
        Door[] allDoors = FindObjectsOfType<Door>();
        foreach (Door d in allDoors)
            d.GetComponent<Collider2D>().enabled = false;

        player.transform.position = targetPos;

        yield return new WaitForSeconds(0.1f);

        Door[] doorsAgain = FindObjectsOfType<Door>();
        foreach (Door d in doorsAgain)
            d.GetComponent<Collider2D>().enabled = true;
    }
}
