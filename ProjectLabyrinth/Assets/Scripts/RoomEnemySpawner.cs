using System.Collections.Generic;
using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs;
    public int minEnemies = 2;
    public int maxEnemies = 5;

    [Header("Placement Rules")]
    public float doorAvoidDistance = 1.2f;  // how far from doors
    public float enemySpacing      = 1.2f;  // min distance between enemies
    public float wallMargin        = 0.4f;  // keep away from room edges

    [Header("Collision Masks")]
    public LayerMask obstacleMask; // Rocks / walls
    public LayerMask doorMask;     // Doors

    [Header("Grid Settings")]
    public int gridWidth  = 9;
    public int gridHeight = 5;

    private Transform roomTransform;
    private DungeonGenerator.RoomType roomType;

    public void InitializeSpawner(Transform room, DungeonGenerator.RoomType type)
    {
        roomTransform = room;
        roomType = type;
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        // no combat in start/boss or if no prefabs
        if (roomType == DungeonGenerator.RoomType.Start ||
            roomType == DungeonGenerator.RoomType.Boss ||
            enemyPrefabs == null || enemyPrefabs.Length == 0)
            return;

        Bounds roomBounds = GetRoomBounds();

        // build list of all valid tiles in this room
        List<Vector2> candidateTiles = BuildCandidateTiles(roomBounds);
        if (candidateTiles.Count == 0)
            return;

        // how many can we realistically spawn
        int desired = Random.Range(minEnemies, maxEnemies + 1);
        int spawnCount = Mathf.Min(desired, candidateTiles.Count);

        // shuffle tile list
        Shuffle(candidateTiles);

        // pick tiles while enforcing spacing
        List<Vector2> chosen = new List<Vector2>();
        foreach (Vector2 tilePos in candidateTiles)
        {
            if (!IsFarEnough(tilePos, chosen)) continue;

            chosen.Add(tilePos);
            if (chosen.Count >= spawnCount)
                break;
        }

        if (chosen.Count == 0)
            return;

        // spawn enemies
        RoomController roomCtrl = roomTransform.GetComponent<RoomController>();

        foreach (Vector2 pos in chosen)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(prefab, pos, Quaternion.identity, roomTransform);

            // if RoomController exists, let it manage activation/locking
            if (roomCtrl != null)
                roomCtrl.RegisterEnemy(enemy);
        }
    }

    // Build a grid over the room and collect tiles that are open
    private List<Vector2> BuildCandidateTiles(Bounds roomBounds)
    {
        List<Vector2> tiles = new List<Vector2>();

        float stepX = roomBounds.size.x / gridWidth;
        float stepY = roomBounds.size.y / gridHeight;

        float checkRadius = Mathf.Min(stepX, stepY) * 0.4f;

        for (int gy = 0; gy < gridHeight; gy++)
        {
            for (int gx = 0; gx < gridWidth; gx++)
            {
                Vector2 center = new Vector2(
                    roomBounds.min.x + (gx + 0.5f) * stepX,
                    roomBounds.min.y + (gy + 0.5f) * stepY
                );

                // keep away from room edges
                if (Mathf.Abs(center.x - roomBounds.center.x) > roomBounds.extents.x - wallMargin) continue;
                if (Mathf.Abs(center.y - roomBounds.center.y) > roomBounds.extents.y - wallMargin) continue;

                // blocked by rocks/walls?
                if (Physics2D.OverlapCircle(center, checkRadius, obstacleMask))
                    continue;

                // too close to a door?
                if (doorMask.value != 0 &&
                    Physics2D.OverlapCircle(center, doorAvoidDistance, doorMask))
                    continue;

                tiles.Add(center);
            }
        }

        return tiles;
    }

    private bool IsFarEnough(Vector2 pos, List<Vector2> others)
    {
        foreach (var p in others)
        {
            if (Vector2.Distance(pos, p) < enemySpacing)
                return false;
        }
        return true;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private Bounds GetRoomBounds()
    {
        Collider2D col = roomTransform.GetComponent<Collider2D>();
        if (col != null)
            return col.bounds;

        return new Bounds(roomTransform.position, new Vector3(10, 6, 0));
    }
}
