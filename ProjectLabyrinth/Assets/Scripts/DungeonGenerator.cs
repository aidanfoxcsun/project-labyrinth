using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public enum RoomType { Normal, Start, Boss, Treasure } 

    [Header("Prefabs")]
    public GameObject roomPrefab;

    [Header("Rock Settings")]
    public GameObject[] rockPrefabs;
    public int minRocksPerRoom = 2;
    public int maxRocksPerRoom = 5;


    [Header("Generation Settings")]
    public int seed = 0;
    public int targetRooms = 12;
    public int maxBranchDegree = 3;
    public int minBossDistance = 5; 

  [Header("Room Layout")]
    public Vector2 roomSpacing = new Vector2(12f, 8f);
    public bool autoDetectSpacing = true;     // auto-measure from prefab
    public float verticalOverlap = 0.15f;     // small seam killer




    private Dictionary<Vector2Int, GameObject> spawnedRooms = new();
    private HashSet<Vector2Int> occupied = new();
    private Dictionary<Vector2Int, RoomType> roomTypes = new(); 

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
        // Make a temp instance to measure actual render bounds
        var temp = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
        Bounds b = GetRenderBounds(temp);
        roomSpacing = new Vector2(b.size.x, b.size.y - verticalOverlap);
        DestroyImmediate(temp);
    }
}

// Encapsulate all renderers (SpriteRenderer, MeshRenderer, etc.)
Bounds GetRenderBounds(GameObject root)
{
    var rends = root.GetComponentsInChildren<Renderer>();
    var bounds = new Bounds(root.transform.position, Vector3.zero);
    foreach (var r in rends) bounds.Encapsulate(r.bounds);
    return bounds;
}



    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        // Seed RNG
        Random.InitState(seed == 0 ? System.DateTime.Now.Millisecond : seed);

        // Start room at origin
        Vector2Int startPos = Vector2Int.zero;
        List<Vector2Int> frontier = new List<Vector2Int> { startPos };
        occupied.Add(startPos);

        int placed = 1;

        while (placed < targetRooms && frontier.Count > 0)
        {
            Vector2Int current = frontier[Random.Range(0, frontier.Count)];
            bool expanded = false;

            // Shuffle directions
            List<int> order = new List<int> { 0, 1, 2, 3 };
            for (int i = 0; i < order.Count; i++)
            {
                int j = Random.Range(i, order.Count);
                (order[i], order[j]) = (order[j], order[i]);
            }

            foreach (int idx in order)
            {
                Vector2Int next = current + directions[idx];
                if (occupied.Contains(next)) continue;

                int neighborCount = CountOccupiedNeighbors(next);
                if (neighborCount > 1) continue;

                occupied.Add(next);
                frontier.Add(next);
                placed++;
                expanded = true;
                break;
            }

            if (!expanded) frontier.Remove(current);
        }

        // Assign room types 
        AssignRoomTypes(startPos);

        // Spawn the rooms with correct door masks
        foreach (Vector2Int pos in occupied)
        {
            int mask = ComputeDoorMask(pos);
            SpawnRoom(pos, mask, roomTypes[pos]); // pass type
        }

        // Debug log
        Debug.Log("Generated " + occupied.Count + " rooms");

        // Recenter camera on Start room
        if (Camera.main != null)
        {
            Vector3 camPos = new Vector3(0, 0, -10); // start is always at (0,0)
            Camera.main.transform.position = camPos;

            // Adjust zoom based on map size
            int maxExtent = 0;
            foreach (var pos in occupied)
            {
                maxExtent = Mathf.Max(maxExtent, Mathf.Abs(pos.x), Mathf.Abs(pos.y));
            }
            Camera.main.orthographicSize = Mathf.Max(5, maxExtent * 2);
        }
    }

    // Assign Start, Boss, Treasure 
    void AssignRoomTypes(Vector2Int startPos)
    {
        foreach (var pos in occupied) roomTypes[pos] = RoomType.Normal;
        roomTypes[startPos] = RoomType.Start;

        // BFS for distances
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

        // Boss = farthest room
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

        // Treasure = items (Left it here but currently inactive until ready to be worked on)
        Vector2Int treasure = startPos;
        int treasureDist = -1;
        foreach (var kv in dist)
        {
            if (kv.Value > treasureDist && roomTypes[kv.Key] == RoomType.Normal)
            {
                treasureDist = kv.Value;
                treasure = kv.Key;
            }
        }

        // Treasure Room Disabled currently until we're ready to work on items
        // if (treasureDist > 0)
        // roomTypes[treasure] = RoomType.Treasure;
    }

    void SpawnRoom(Vector2Int gridPos, int doorMask, RoomType type)
    {
        if (spawnedRooms.ContainsKey(gridPos)) return;

        Vector3 worldPos = new Vector3(
            gridPos.x * roomSpacing.x,
            gridPos.y * (roomSpacing.y),
            0
        );


        GameObject room = Instantiate(roomPrefab, worldPos, Quaternion.identity, transform);

        // Activate doors + set their parent grid/direction
        if ((doorMask & 1) != 0)
{
    var door = room.transform.Find("Door_N").GetComponent<Door>();
    door.gameObject.SetActive(true);
    door.direction = new Vector2Int(0, 1);
    door.parentGrid = gridPos;

    //disable wall behind the north door
    var wall = room.transform.Find("Wall_N");
    if (wall != null) wall.gameObject.SetActive(false);
}

if ((doorMask & 2) != 0)
{
    var door = room.transform.Find("Door_E").GetComponent<Door>();
    door.gameObject.SetActive(true);
    door.direction = new Vector2Int(1, 0);
    door.parentGrid = gridPos;

    var wall = room.transform.Find("Wall_E");
    if (wall != null) wall.gameObject.SetActive(false);
}

if ((doorMask & 4) != 0)
{
    var door = room.transform.Find("Door_S").GetComponent<Door>();
    door.gameObject.SetActive(true);
    door.direction = new Vector2Int(0, -1);
    door.parentGrid = gridPos;

    var wall = room.transform.Find("Wall_S");
    if (wall != null) wall.gameObject.SetActive(false);
}

if ((doorMask & 8) != 0)
{
    var door = room.transform.Find("Door_W").GetComponent<Door>();
    door.gameObject.SetActive(true);
    door.direction = new Vector2Int(-1, 0);
    door.parentGrid = gridPos;

    var wall = room.transform.Find("Wall_W");
    if (wall != null) wall.gameObject.SetActive(false);
}


        // Color rooms based on type
        var sr = room.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            switch (type)
            {
                case RoomType.Start: sr.color = Color.green; break;
                case RoomType.Boss: sr.color = Color.red; break;
                case RoomType.Treasure: sr.color = Color.yellow; break;
                default: sr.color = Color.white; break;
            }

            // Hide overlapping walls between connected rooms
            Transform wallN = room.transform.Find("Wall_N");
            Transform wallE = room.transform.Find("Wall_E");
            Transform wallS = room.transform.Find("Wall_S");
            Transform wallW = room.transform.Find("Wall_W");

// Disable walls where a neighboring room exists (to avoid overlap)
            if ((doorMask & 1) != 0 && wallN != null) wallN.gameObject.SetActive(false); // North neighbor
            if ((doorMask & 2) != 0 && wallE != null) wallE.gameObject.SetActive(false); // East neighbor
            if ((doorMask & 4) != 0 && wallS != null) wallS.gameObject.SetActive(false); // South neighbor
            if ((doorMask & 8) != 0 && wallW != null) wallW.gameObject.SetActive(false); // West neighbor

        }

       // Spawn random rocks in all non-start rooms
        SpawnRocksInRoom(gridPos, room, type);



        // Save room reference
        spawnedRooms[gridPos] = room;
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
    bool hasEastDoor  = HasRoom(gridPos + new Vector2Int(1, 0));
    bool hasWestDoor  = HasRoom(gridPos + new Vector2Int(-1, 0));

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
    int[,] dirs = { {1,0}, {-1,0}, {0,1}, {0,-1} };

    while (q.Count > 0)
    {
        var c = q.Dequeue();
        reachable++;
        for (int i = 0; i < 4; i++)
        {
            int nx = c.x + dirs[i,0];
            int ny = c.y + dirs[i,1];
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





    int ComputeDoorMask(Vector2Int pos)
    {
        int mask = 0;
        if (occupied.Contains(pos + directions[0])) mask |= 1; // N
        if (occupied.Contains(pos + directions[1])) mask |= 2; // E
        if (occupied.Contains(pos + directions[2])) mask |= 4; // S
        if (occupied.Contains(pos + directions[3])) mask |= 8; // W
        return mask;
    }

    int CountOccupiedNeighbors(Vector2Int pos)
    {
        int count = 0;
        foreach (var d in directions)
            if (occupied.Contains(pos + d)) count++;
        return count;
    }

    public bool HasRoom(Vector2Int pos)
    {
        return spawnedRooms.ContainsKey(pos);
    }

    public GameObject GetRoom(Vector2Int pos)
    {
        return spawnedRooms.ContainsKey(pos) ? spawnedRooms[pos] : null;
    }
}
