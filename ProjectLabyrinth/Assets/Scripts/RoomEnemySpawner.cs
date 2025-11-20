using System.Collections.Generic;
using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs;
    public int minEnemies = 2;
    public int maxEnemies = 5;
    public float spawnPadding = 1.5f;
    public float doorAvoidDistance = 2f;
    public float enemySpacing = 1.5f;
    public LayerMask obstacleMask; // Rocks
    public LayerMask doorMask;     // Doors

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
        // Skip spawning in special rooms
        if (roomType == DungeonGenerator.RoomType.Start ||
            roomType == DungeonGenerator.RoomType.Boss ||
            enemyPrefabs == null || enemyPrefabs.Length == 0)
            return;

        int count = Random.Range(minEnemies, maxEnemies + 1);
        Bounds roomBounds = GetRoomBounds();
        List<Vector2> placedPositions = new List<Vector2>();

        int safetyLimit = count * 15;
        int attempts = 0;

        while (placedPositions.Count < count && attempts < safetyLimit)
        {
            attempts++;

            // ✅ Get a grid-based valid position instead of random bounds
            Vector2 pos = GetValidSpawnPointFromGrid(roomBounds, roomTransform);

            if (!IsValidSpawnPosition(pos, placedPositions))
                continue;

            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity, roomTransform);
            DisableEnemyAI(enemy);

            placedPositions.Add(pos);
        }
    }

    // 🔹 Grid-based spawn point finder
    private Vector2 GetValidSpawnPointFromGrid(Bounds roomBounds, Transform room)
    {
        // Get all rock colliders in this room
        Collider2D[] rocks = room.GetComponentsInChildren<Collider2D>();

        int width = 9;
        int height = 5;
        float tileSizeX = roomBounds.size.x / width;
        float tileSizeY = roomBounds.size.y / height;

        List<Vector2> validTiles = new List<Vector2>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 testPos = new Vector2(
                    roomBounds.min.x + (x + 0.5f) * tileSizeX,
                    roomBounds.min.y + (y + 0.5f) * tileSizeY
                );

                bool blocked = false;
                foreach (var rock in rocks)
                {
                    if (rock.OverlapPoint(testPos))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                    validTiles.Add(testPos);
            }
        }

        if (validTiles.Count == 0)
            return room.position; // fallback center

        return validTiles[Random.Range(0, validTiles.Count)];
    }

    // 🔹 Validity checks for spacing and doors
    private bool IsValidSpawnPosition(Vector2 pos, List<Vector2> placedPositions)
    {
        // Avoid near doors
        if (Physics2D.OverlapCircle(pos, doorAvoidDistance, doorMask))
            return false;

        // Keep distance from other enemies
        foreach (Vector2 existing in placedPositions)
        {
            if (Vector2.Distance(pos, existing) < enemySpacing)
                return false;
        }

        return true;
    }

    
    private void DisableEnemyAI(GameObject enemy)
    {
        foreach (var comp in enemy.GetComponentsInChildren<MonoBehaviour>(true))
        {
            string name = comp.GetType().Name;
            if (name.Contains("Behavior") || name.Contains("Navigation") || name.Contains("Controller"))
                comp.enabled = false;
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
