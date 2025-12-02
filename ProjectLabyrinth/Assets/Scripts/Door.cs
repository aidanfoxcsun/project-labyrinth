using UnityEngine;
using System.Collections;
using System.Linq;

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
        if (generator == null) return;

        Vector2Int nextGrid = parentGrid + direction;
        if (!generator.HasRoom(nextGrid))
        {
            Debug.LogWarning($"[Door] No connected room from {parentGrid} toward {direction}");
            return;
        }

        GameObject targetRoom = generator.GetRoom(nextGrid);

        // 🔍 Add this debug line here
        Debug.Log($"[Door TRACE] {name}  parent={parentGrid}  dir={direction}  next={nextGrid}  " +
                  $"targetRoom={(targetRoom ? targetRoom.name : "NULL")}  " +
                  $"thisRoom={(generator.GetRoom(parentGrid)?.name ?? "NULL")}");

        if (targetRoom == null)
        {
            Debug.LogWarning($"[Door] Target room at {nextGrid} is null.");
            return;
        }

            Vector2Int oppositeDir = -direction;
            string oppositeDoorName = oppositeDir == Vector2Int.up ? "Door_N" :
                                      oppositeDir == Vector2Int.down ? "Door_S" :
                                      oppositeDir == Vector2Int.right ? "Door_E" :
                                      "Door_W";

        // Current suffix
        string thisDoor = gameObject.name;
        string suffix = thisDoor.EndsWith("2") ? "2" :
                        thisDoor.EndsWith("1") ? "1" : "";

        Transform entry = null;
        Collider2D targetDoorCollider = null;

        // 1️⃣ Exact match (Door_W1 → Door_E1)
        if (!string.IsNullOrEmpty(suffix))
            entry = targetRoom.transform.Find($"{baseDoor}{suffix}/EntryPoint");

        // 2️⃣ Fallback to plain door (Door_E)
        if (entry == null)
            entry = targetRoom.transform.Find($"{baseDoor}/EntryPoint");

        // 3️⃣ If still null (normal → large), pick closest
        if (entry == null)
        {
            var candidates = targetRoom
                .GetComponentsInChildren<Transform>(true)
                .Where(t => t.name == "EntryPoint" && t.parent.name.StartsWith(baseDoor))
                .ToArray();

            if (candidates.Length > 0)
            {
                Vector3 myPos = transform.position;
                entry = candidates
                    .OrderBy(t => Vector3.Distance(myPos, t.position))
                    .First();
            }
        }

        if (entry == null)
        {
            Debug.LogWarning($"[Door] EntryPoint not found in {targetRoom.name} for {baseDoor}{suffix}");
            return;
        }

        // Grab the collider of the matching target door (its parent)
        targetDoorCollider = entry.parent.GetComponent<Collider2D>();

        StartCoroutine(TeleportPlayer(other.gameObject, entry.position, targetDoorCollider));
    }

    private IEnumerator TeleportPlayer(GameObject player, Vector3 targetPos, Collider2D targetDoorCollider)
    {
        Door[] allDoors = FindObjectsOfType<Door>();
        foreach (Door d in allDoors)
            d.GetComponent<Collider2D>().enabled = false;
        if (targetDoorCollider) targetDoorCollider.enabled = false;

        player.transform.position = targetPos;

        yield return new WaitForSeconds(0.1f);

        Door[] doorsAgain = FindObjectsOfType<Door>();
        foreach (Door d in doorsAgain)
            d.GetComponent<Collider2D>().enabled = true;
        if (targetDoorCollider) targetDoorCollider.enabled = true;
    }
}
