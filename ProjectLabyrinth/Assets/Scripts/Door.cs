using UnityEngine;
using System.Collections;
using System.Linq;

public class Door : MonoBehaviour
{
    public Vector2Int direction;
    public Vector2Int parentGrid;
    private DungeonGenerator generator;

    void Start() => generator = FindObjectOfType<DungeonGenerator>();

    private void OnTriggerEnter2D(Collider2D other)
    {
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

        // Opposite direction
        Vector2Int opp = -direction;
        string baseDoor = opp == Vector2Int.up ? "Door_N" :
                          opp == Vector2Int.down ? "Door_S" :
                          opp == Vector2Int.right ? "Door_E" :
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

        // --- nudge player slightly outward from the door to prevent re-trigger ---
        Vector3 offset = Vector3.zero;
        if (targetDoorCollider != null)
        {
            string n = targetDoorCollider.name;
            if (n.Contains("N")) offset = Vector3.up * 0.8f;
            else if (n.Contains("S")) offset = Vector3.down * 0.8f;
            else if (n.Contains("E")) offset = Vector3.right * 0.8f;
            else if (n.Contains("W")) offset = Vector3.left * 0.8f;
        }

        player.transform.position = targetPos + offset;

        yield return new WaitForSeconds(0.15f);

        foreach (Door d in allDoors)
            d.GetComponent<Collider2D>().enabled = true;
        if (targetDoorCollider) targetDoorCollider.enabled = true;
    }
}
