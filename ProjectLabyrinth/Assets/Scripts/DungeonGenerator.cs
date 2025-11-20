using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public enum RoomType { Normal, Start, Boss, Treasure }

    [Header("Prefabs")]
    public GameObject roomPrefab;
    public GameObject largeRoomPrefab;

    [Header("Rock Settings")]
    public GameObject[] rockPrefabs;
    public int minRocksPerRoom = 2;
    public int maxRocksPerRoom = 5;

    [Header("Enemy Spawning")]
    public GameObject[] enemyPrefabs;

    [Header("Enemy Spawning Settings")]
    public int minEnemiesPerRoom = 2;
    public int maxEnemiesPerRoom = 5;



    [Header("Generation Settings")]
    public int seed = 0;
    public int targetRooms = 12;
    public int minBossDistance = 5;

    [Header("Room Layout")]
    public Vector2 roomSpacing = new Vector2(12f, 8f);
    public bool autoDetectSpacing = true;
    public float verticalOverlap = 0.15f;

    private Dictionary<Vector2Int, GameObject> spawnedRooms = new();
    private HashSet<Vector2Int> occupied = new();
    private HashSet<Vector2Int> occupiedByLargeRooms = new();
    private Dictionary<Vector2Int, RoomType> roomTypes = new();

    private HashSet<Vector2Int> largeRoomOrigins = new();



    private static readonly Vector2Int[] directions = {
        new Vector2Int(0, 1),   // N
        new Vector2Int(1, 0),   // E
        new Vector2Int(0, -1),  // S
        new Vector2Int(-1, 0)   // W
    };

    void Awake()
    {
        if (autoDetectSpacing && roomPrefab != null)
        {
            var temp = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
            Bounds b = GetRenderBounds(temp);
            roomSpacing = new Vector2(b.size.x, b.size.y - verticalOverlap);
            DestroyImmediate(temp);
        }
    }

    Bounds GetRenderBounds(GameObject root)
    {
        var rends = root.GetComponentsInChildren<Renderer>();
        var bounds = new Bounds(root.transform.position, Vector3.zero);
        foreach (var r in rends) bounds.Encapsulate(r.bounds);
        return bounds;
    }

    void Start() => GenerateDungeon();

    void GenerateDungeon()
    {
        Random.InitState(seed == 0 ? System.DateTime.Now.Millisecond : seed);

        Vector2Int startPos = Vector2Int.zero;
        List<Vector2Int> frontier = new() { startPos };
        occupied.Add(startPos);
        int placed = 1;

        while (placed < targetRooms && frontier.Count > 0)
{
    Vector2Int current = frontier[Random.Range(0, frontier.Count)];
    bool expanded = false;

    List<int> order = new() { 0, 1, 2, 3 };
    for (int i = 0; i < order.Count; i++)
    {
        int j = Random.Range(i, order.Count);
        (order[i], order[j]) = (order[j], order[i]);
    }

    foreach (int idx in order)
    {
        Vector2Int next = current + directions[idx];
        if (occupied.Contains(next)) continue;
        if (CountOccupiedNeighbors(next) > 1) continue;

        // Try 2×2 first
        bool placeLarge = Random.value < 0.25f && largeRoomPrefab != null && CanPlaceLargeRoom(next);
        if (placeLarge)
{
    MarkLargeRoomOccupied(next);
    frontier.Add(next);
    placed += 4;
    expanded = true;
    continue;
}


        // Otherwise small
        occupied.Add(next);
        frontier.Add(next);
        placed++;
        expanded = true;
        break;
    }

    if (!expanded)
        frontier.Remove(current);
}

        AssignRoomTypes(startPos);

        foreach (Vector2Int pos in occupied)
{
    // Skip if already spawned
    if (spawnedRooms.ContainsKey(pos)) continue;

    // Skip if this cell is part of a large room that has already been spawned
    if (occupiedByLargeRooms.Contains(pos) && !IsLargeRoomOrigin(pos))
        continue;

    int mask = ComputeDoorMask(pos);
    SpawnRoom(pos, mask, roomTypes[pos]);
}


        Debug.Log($"Generated {occupied.Count} rooms (including large ones).");
    }

    // ======================== ROOM TYPE LOGIC ========================
    void AssignRoomTypes(Vector2Int startPos)
    {
        foreach (var pos in occupied)
            roomTypes[pos] = RoomType.Normal;
        roomTypes[startPos] = RoomType.Start;

        Dictionary<Vector2Int, int> dist = new();
        Queue<Vector2Int> q = new();
        q.Enqueue(startPos);
        dist[startPos] = 0;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var d in directions)
            {
                var nb = cur + d;
                if (occupied.Contains(nb) && !dist.ContainsKey(nb))
                {
                    dist[nb] = dist[cur] + 1;
                    q.Enqueue(nb);
                }
            }
        }

        Vector2Int farthest = startPos;
        int maxDist = 0;
        foreach (var kv in dist)
        {
            if (kv.Value > maxDist)
            {
                maxDist = kv.Value;
                farthest = kv.Key;
            }
        }

        if (maxDist >= minBossDistance)
            roomTypes[farthest] = RoomType.Boss;
    }

    // ======================== SPAWN ROOMS ========================
    void SpawnRoom(Vector2Int gridPos, int doorMask, RoomType type)
    {

        // Skip small rooms if this cell is inside a large room but not its origin
        if (occupiedByLargeRooms.Contains(gridPos) && !IsLargeRoomOrigin(gridPos))
         return;

        // Skip duplicates (large or small)
        if (spawnedRooms.ContainsKey(gridPos))
        return;


        bool isLargeRoom = IsLargeRoomOrigin(gridPos);
        GameObject prefab = isLargeRoom ? largeRoomPrefab : roomPrefab;

        Vector3 worldPos = isLargeRoom
            ? new Vector3(gridPos.x * roomSpacing.x + roomSpacing.x / 2, gridPos.y * roomSpacing.y + roomSpacing.y / 2, 0)
            : new Vector3(gridPos.x * roomSpacing.x, gridPos.y * roomSpacing.y, 0);

        GameObject room = Instantiate(prefab, worldPos, Quaternion.identity, transform);

        var sr = room.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = type switch
            {
                RoomType.Start => Color.green,
                RoomType.Boss => Color.red,
                _ => Color.white
            };
        }

        if (isLargeRoom)
            ApplyDoubleDoors(room, gridPos);
        else
            ApplyDoors(room, gridPos, doorMask);

        SpawnRocksInRoom(gridPos, room, type);

// === Enemy Spawner Integration ===
RoomEnemySpawner spawner = room.GetComponent<RoomEnemySpawner>();
if (spawner == null)
    spawner = room.AddComponent<RoomEnemySpawner>();

spawner.enemyPrefabs = enemyPrefabs;
spawner.minEnemies = minEnemiesPerRoom;
spawner.maxEnemies = maxEnemiesPerRoom;

// Hook up LayerMasks directly (no Inspector setup needed)
spawner.obstacleMask = LayerMask.GetMask("Rocks");
spawner.doorMask = LayerMask.GetMask("Doors");

spawner.InitializeSpawner(room.transform, type);
        
if (isLargeRoom)
{
    Vector2Int[] offsets =
    {
        Vector2Int.zero,
        Vector2Int.right,
        Vector2Int.up,
        new Vector2Int(1, 1)
    };

    foreach (var o in offsets)
    {
        Vector2Int cell = gridPos + o;
        if (!spawnedRooms.ContainsKey(cell))
            spawnedRooms[cell] = room;
    }
}
else
{
    spawnedRooms[gridPos] = room;
}

    }

void ApplyDoors(GameObject room, Vector2Int gridPos, int doorMask)
{
    if ((doorMask & 1) != 0) EnableDoor(room, gridPos, "Door_N", "Wall_N", new Vector2Int(0, 1));
    if ((doorMask & 2) != 0) EnableDoor(room, gridPos, "Door_E", "Wall_E", new Vector2Int(1, 0));
    if ((doorMask & 4) != 0) EnableDoor(room, gridPos, "Door_S", "Wall_S", new Vector2Int(0, -1));
    if ((doorMask & 8) != 0) EnableDoor(room, gridPos, "Door_W", "Wall_W", new Vector2Int(-1, 0));
}


    void ApplyDoubleDoors(GameObject room, Vector2Int gridPos)
{
    (string name, Vector2Int localOffset, Vector2Int dir)[] doors = {
        ("Door_N1", new Vector2Int(0,1), new Vector2Int(0,1)),
        ("Door_N2", new Vector2Int(1,1), new Vector2Int(0,1)),
        ("Door_S1", new Vector2Int(0,0), new Vector2Int(0,-1)),
        ("Door_S2", new Vector2Int(1,0), new Vector2Int(0,-1)),
        ("Door_E1", new Vector2Int(1,0), new Vector2Int(1,0)),
        ("Door_E2", new Vector2Int(1,1), new Vector2Int(1,0)),
        ("Door_W1", new Vector2Int(0,0), new Vector2Int(-1,0)),
        ("Door_W2", new Vector2Int(0,1), new Vector2Int(-1,0))
    };

    foreach (var (name, offset, dir) in doors)
    {
        var door = room.transform.Find(name);
        if (door == null) continue;

        Vector2Int doorGrid = gridPos + offset;
        Vector2Int nextGrid = doorGrid + dir;

        // Only connect if door leads outside the 2×2 block
        bool connectsOutside =
            !(
                nextGrid.x >= gridPos.x && nextGrid.x <= gridPos.x + 1 &&
                nextGrid.y >= gridPos.y && nextGrid.y <= gridPos.y + 1
            );

        if (connectsOutside)
        {
            door.gameObject.SetActive(true);

            var d = door.GetComponent<Door>();
            if (d != null)
            {
                d.direction = dir;
                d.parentGrid = doorGrid;
            }

            
            var col = door.GetComponent<Collider2D>();
            if (col != null)
            {
                Vector3 nudge = new Vector3(dir.x * -0.1f, dir.y * -0.1f, 0);
                door.transform.position += nudge;
            }
            

            if (!spawnedRooms.ContainsKey(doorGrid))
                spawnedRooms[doorGrid] = room;
        }
    }
}







void TryEnablePair(GameObject room, string aName, string bName, bool shouldEnable, Vector2Int dir, Vector2Int parent)
{
    if (!shouldEnable) return;

    var a = room.transform.Find(aName);
    var b = room.transform.Find(bName);
    if (a != null) { a.gameObject.SetActive(true); var d = a.GetComponent<Door>(); if (d){ d.direction = dir; d.parentGrid = parent; } }
    if (b != null) { b.gameObject.SetActive(true); var d = b.GetComponent<Door>(); if (d){ d.direction = dir; d.parentGrid = parent; } }
}


    void EnableDoor(GameObject room, Vector2Int gridPos, string doorName, string wallName, Vector2Int dir)
    {
        var door = room.transform.Find(doorName);
        if (door == null) return;
        door.gameObject.SetActive(true);

        var d = door.GetComponent<Door>();
        if (d != null)
        {
            d.direction = dir;
            d.parentGrid = gridPos;
        }

        var wall = room.transform.Find(wallName);
        if (wall != null) wall.gameObject.SetActive(false);
    }


    void SpawnRocksInRoom(Vector2Int gridPos, GameObject room, RoomType type)
    {
        if (type == RoomType.Start || type == RoomType.Boss) return;
        if (rockPrefabs == null || rockPrefabs.Length == 0) return;

        Vector3 basePos = room.transform.position;

        // Room layout grid setup
        float halfW = 6f;
        float halfH = 4f;
        float margin = 0.4f;
        float tileSize = 1.2f;
        int width = 9;
        int height = 5;

        // === Isaac-style rock pattern library ===
        List<int[,]> patterns = new List<int[,]>
    {
        new int[,] { {0,0,1,1,1,1,1,0,0}, {0,1,0,0,0,0,0,1,0}, {1,0,0,0,0,0,0,0,1}, {0,1,0,0,0,0,0,1,0}, {0,0,1,1,1,1,1,0,0} },
        new int[,] { {1,1,0,0,0,0,0,1,1}, {1,0,0,0,0,0,0,0,1}, {0,0,0,0,0,0,0,0,0}, {1,0,0,0,0,0,0,0,1}, {1,1,0,0,0,0,0,1,1} },
        new int[,] { {1,0,0,0,0,0,0,0,1}, {1,0,0,0,0,0,0,0,1}, {1,0,0,0,0,0,0,0,1}, {1,0,0,0,0,0,0,0,1}, {1,0,0,0,0,0,0,0,1} },
        new int[,] { {0,0,0,0,0,0,0,0,0}, {1,1,1,1,1,1,1,1,1}, {0,0,0,0,0,0,0,0,0}, {1,1,1,1,1,1,1,1,1}, {0,0,0,0,0,0,0,0,0} },
        new int[,] { {0,0,0,0,0,0,0,0,0}, {0,1,1,1,1,1,1,1,0}, {0,1,1,1,1,1,1,1,0}, {0,1,1,1,1,1,1,1,0}, {0,0,0,0,0,0,0,0,0} },
        new int[,] { {1,0,0,0,0,0,0,0,0}, {0,1,0,0,0,0,0,0,0}, {0,0,1,0,0,0,0,0,0}, {0,0,0,1,0,0,0,0,0}, {0,0,0,0,1,0,0,0,0} },
        new int[,] { {1,0,0,0,0,0,0,0,1}, {0,1,0,0,0,0,0,1,0}, {0,0,1,0,0,0,1,0,0}, {0,1,0,0,0,0,0,1,0}, {1,0,0,0,0,0,0,0,1} },
        new int[,] { {1,1,1,0,0,0,0,0,0}, {1,0,0,0,0,0,0,0,0}, {1,0,0,0,0,0,0,0,0}, {1,0,0,0,0,0,0,0,0}, {1,1,1,0,0,0,0,0,0} },
        new int[,] { {0,0,0,0,0,0,1,1,1}, {0,0,0,0,0,0,0,0,1}, {0,0,0,0,0,0,0,0,1}, {0,0,0,0,0,0,0,0,1}, {0,0,0,0,0,0,1,1,1} },
        new int[,] { {0,1,1,1,0,1,1,1,0}, {1,0,0,0,1,0,0,0,1}, {1,0,0,0,1,0,0,0,1}, {1,0,0,0,1,0,0,0,1}, {0,1,1,1,0,1,1,1,0} },
        new int[,] { {0,0,0,1,1,1,0,0,0}, {0,0,0,1,1,1,0,0,0}, {1,1,1,1,1,1,1,1,1}, {0,0,0,1,1,1,0,0,0}, {0,0,0,1,1,1,0,0,0} },
        new int[,] { {1,0,1,0,1,0,1,0,1}, {0,1,0,1,0,1,0,1,0}, {1,0,1,0,1,0,1,0,1}, {0,1,0,1,0,1,0,1,0}, {1,0,1,0,1,0,1,0,1} },
        new int[,] { {1,1,1,1,1,1,1,1,1}, {1,0,0,0,0,0,0,0,1}, {1,0,0,0,0,0,0,0,1}, {1,0,0,0,0,0,0,0,1}, {1,1,1,1,1,1,1,1,1} },
        new int[,] { {1,0,0,1,0,0,1,0,0}, {0,1,0,0,1,0,0,1,0}, {0,0,1,0,0,1,0,0,1}, {0,1,0,0,1,0,0,1,0}, {1,0,0,1,0,0,1,0,0} },
        new int[,] { {0,0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0,0}, {0,1,1,1,1,1,1,1,0}, {1,1,1,1,1,1,1,1,1}, {1,1,1,1,1,1,1,1,1} },
        new int[,] { {1,1,1,1,1,1,1,1,1}, {1,1,1,1,1,1,1,1,1}, {0,1,1,1,1,1,1,1,0}, {0,0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0,0} },
        new int[,] { {1,1,1,1,1,1,1,1,1}, {1,0,0,0,0,0,0,0,0}, {1,0,0,0,0,0,0,0,0}, {1,0,0,0,0,0,0,0,0}, {1,1,1,1,1,1,1,1,1} },
        new int[,] { {0,0,0,0,0,0,0,0,0}, {1,0,0,0,0,0,0,0,1}, {1,0,0,0,0,0,0,0,1}, {1,0,0,0,0,0,0,0,1}, {1,1,1,1,1,1,1,1,1} },
        new int[,] { {1,1,1,1,1,1,1,1,1}, {1,0,0,0,0,0,0,0,1}, {1,0,0,0,0,0,0,0,1}, {1,0,0,0,0,0,0,0,1}, {0,0,0,0,0,0,0,0,0} },
        new int[,] { {0,1,0,0,1,0,0,1,0}, {1,0,0,1,0,0,1,0,1}, {0,0,1,0,0,1,0,0,0}, {1,0,0,0,1,0,0,0,1}, {0,1,0,1,0,1,0,1,0} }
    };

        // Pick a valid pattern
        int[,] pattern = patterns[Random.Range(0, patterns.Count)];
        if (!IsPatternWalkable(pattern))
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                pattern = patterns[Random.Range(0, patterns.Count)];
                if (IsPatternWalkable(pattern)) break;
            }
        }

        // Grid shrink for nicer spacing
        float gridShrinkX = 0.82f;
        float gridShrinkY = 0.85f;

        // Door clearance configuration
        float doorWidth = 1.8f;
        float doorHeight = 1.9f;
        float doorDepth = 1.9f;

        bool hasNorthDoor = HasRoom(gridPos + new Vector2Int(0, 1));
        bool hasSouthDoor = HasRoom(gridPos + new Vector2Int(0, -1));
        bool hasEastDoor = HasRoom(gridPos + new Vector2Int(1, 0));
        bool hasWestDoor = HasRoom(gridPos + new Vector2Int(-1, 0));

        // === Place rocks ===
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pattern[y, x] == 0) continue;

                float px = (x - (width - 1) / 2f) * tileSize * gridShrinkX;
                float py = (y - (height - 1) / 2f) * tileSize * gridShrinkY;

                // Keep doors clear
                if (hasNorthDoor && py > halfH - doorDepth && Mathf.Abs(px) < doorWidth) continue;
                if (hasSouthDoor && py < -halfH + doorDepth && Mathf.Abs(px) < doorWidth) continue;
                if (hasEastDoor && px > halfW - doorDepth && Mathf.Abs(py) < doorHeight) continue;
                if (hasWestDoor && px < -halfW + doorDepth && Mathf.Abs(py) < doorHeight) continue;

                if (Mathf.Abs(px) > halfW - margin || Mathf.Abs(py) > halfH - margin) continue;

                Vector3 pos = basePos + new Vector3(px, py, 0);
                GameObject rockPrefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
                Instantiate(rockPrefab, pos, Quaternion.identity, room.transform);
            }
        }
    }

    bool IsPatternWalkable(int[,] grid)
    {
        int width = grid.GetLength(1);
        int height = grid.GetLength(0);
        bool[,] visited = new bool[width, height];

        // Find an open start cell
        Vector2Int start = new Vector2Int(-1, -1);
        for (int y = 0; y < height && start.x == -1; y++)
            for (int x = 0; x < width; x++)
                if (grid[y, x] == 0) { start = new Vector2Int(x, y); break; }

        if (start.x == -1) return false;

        Queue<Vector2Int> q = new();
        q.Enqueue(start);
        visited[start.x, start.y] = true;

        int reachable = 0;
        int[,] dirs = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

        while (q.Count > 0)
        {
            var c = q.Dequeue();
            reachable++;
            for (int i = 0; i < 4; i++)
            {
                int nx = c.x + dirs[i, 0];
                int ny = c.y + dirs[i, 1];
                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                if (visited[nx, ny] || grid[ny, nx] == 1) continue;
                visited[nx, ny] = true;
                q.Enqueue(new Vector2Int(nx, ny));
            }
        }

        float openRatio = (float)reachable / (width * height);
        return openRatio >= 0.4f;
    }




    // === Helper Methods for Pattern Transforms ===
    int[,] MirrorPattern(int[,] pattern, bool horizontal, bool vertical)
    {
        int h = pattern.GetLength(0);
        int w = pattern.GetLength(1);
        int[,] result = new int[h, w];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                result[y, x] = pattern[vertical ? (h - 1 - y) : y, horizontal ? (w - 1 - x) : x];
        return result;
    }

    int[,] RotatePattern(int[,] pattern, int angle)
    {
        int h = pattern.GetLength(0);
        int w = pattern.GetLength(1);
        int[,] result = angle % 180 == 0 ? new int[h, w] : new int[w, h];

        switch (angle)
        {
            case 90:
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        result[x, h - 1 - y] = pattern[y, x];
                break;
            case 180:
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        result[h - 1 - y, w - 1 - x] = pattern[y, x];
                break;
            case 270:
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        result[w - 1 - x, y] = pattern[y, x];
                break;
            default:
                return pattern;
        }
        return result;
    }





    int CountOccupiedNeighbors(Vector2Int pos)
    {
        int c = 0;
        foreach (var d in directions)
            if (occupied.Contains(pos + d)) c++;
        return c;
    }

    int ComputeDoorMask(Vector2Int pos)
{
    int mask = 0;
    bool IsOccupied(Vector2Int p) => occupied.Contains(p) || occupiedByLargeRooms.Contains(p);

    if (IsOccupied(pos + directions[0])) mask |= 1; // N
    if (IsOccupied(pos + directions[1])) mask |= 2; // E
    if (IsOccupied(pos + directions[2])) mask |= 4; // S
    if (IsOccupied(pos + directions[3])) mask |= 8; // W

    return mask;
}


    bool IsLargeRoomOrigin(Vector2Int pos)
{
    return largeRoomOrigins.Contains(pos);
}


    public bool HasRoom(Vector2Int pos)
{
    // Treat both normal and large-room cells as occupied
    return occupied.Contains(pos) || occupiedByLargeRooms.Contains(pos);
}


   public GameObject GetRoom(Vector2Int pos)
{
    // Try direct lookup first
    if (spawnedRooms.TryGetValue(pos, out GameObject room))
        return room;

    
    foreach (var origin in largeRoomOrigins)
    {
        if (pos.x >= origin.x && pos.x <= origin.x + 1 &&
            pos.y >= origin.y && pos.y <= origin.y + 1)
        {
            if (spawnedRooms.TryGetValue(origin, out GameObject largeRoom))
                return largeRoom;
        }
    }

    return null;
}


    bool CanPlaceLargeRoom(Vector2Int origin)
{
    // Check if any of the 4 tiles are already occupied by ANY room
    Vector2Int[] cells =
    {
        origin,
        origin + Vector2Int.right,
        origin + Vector2Int.up,
        origin + new Vector2Int(1, 1)
    };

    foreach (var c in cells)
        if (occupied.Contains(c))
            return false;

    return true;
}




void MarkLargeRoomOccupied(Vector2Int origin)
{
    largeRoomOrigins.Add(origin);

    Vector2Int[] cells =
    {
        origin,
        origin + Vector2Int.right,
        origin + Vector2Int.up,
        origin + new Vector2Int(1, 1)
    };

    foreach (var c in cells)
    {
        occupied.Add(c);
        occupiedByLargeRooms.Add(c);
    }
}






}