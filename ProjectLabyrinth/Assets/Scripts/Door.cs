using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    public Vector2Int direction;
    public Vector2Int parentGrid;
    private DungeonGenerator generator;

    void Start() => generator = FindObjectOfType<DungeonGenerator>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Vector2Int nextGrid = parentGrid + direction;

        if (generator != null && generator.HasRoom(nextGrid))
        {
            GameObject targetRoom = generator.GetRoom(nextGrid);

            // Opposite direction/door
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
        // Disable all door colliders for a brief moment to avoid possible bugs
        Door[] allDoors = FindObjectsOfType<Door>();
        foreach (Door d in allDoors)
            d.GetComponent<Collider2D>().enabled = false;

        // Moves player
        player.transform.position = targetPos;

        // Wait a short moment to fully exit triggers
        yield return new WaitForSeconds(0.1f);

        // Re-enable colliders
        Door[] doorsAgain = FindObjectsOfType<Door>();
        foreach (Door d in doorsAgain)
            d.GetComponent<Collider2D>().enabled = true;
    }
}
