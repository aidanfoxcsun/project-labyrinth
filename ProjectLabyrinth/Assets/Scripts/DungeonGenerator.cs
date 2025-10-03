using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public enum RoomType { Normal, Start, Boss, Treasure } 

    [Header("Prefabs")]
    public GameObject roomPrefab;

    [Header("Generation Settings")]
    public int seed = 0;
    public int targetRooms = 12;
    public int maxBranchDegree = 3;
    public int minBossDistance = 5; 

    private Dictionary<Vector2Int, GameObject> spawnedRooms = new();
    private HashSet<Vector2Int> occupied = new();
    private Dictionary<Vector2Int, RoomType> roomTypes = new(); 

    private static readonly Vector2Int[] directions = {
        new Vector2Int(0, 1),   // N
        new Vector2Int(1, 0),   // E
        new Vector2Int(0, -1),  // S
        new Vector2Int(-1, 0)   // W
    };

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

        Vector3 worldPos = new Vector3(gridPos.x * 6, gridPos.y * 6, 0);
        GameObject room = Instantiate(roomPrefab, worldPos, Quaternion.identity, transform);

        // Activate doors + set their parent grid/direction
        if ((doorMask & 1) != 0)
        {
            var door = room.transform.Find("Door_N").GetComponent<Door>();
            door.gameObject.SetActive(true);
            door.direction = new Vector2Int(0, 1);
            door.parentGrid = gridPos;
        }

        if ((doorMask & 2) != 0)
        {
            var door = room.transform.Find("Door_E").GetComponent<Door>();
            door.gameObject.SetActive(true);
            door.direction = new Vector2Int(1, 0);
            door.parentGrid = gridPos;
        }

        if ((doorMask & 4) != 0)
        {
            var door = room.transform.Find("Door_S").GetComponent<Door>();
            door.gameObject.SetActive(true);
            door.direction = new Vector2Int(0, -1);
            door.parentGrid = gridPos;
        }

        if ((doorMask & 8) != 0)
        {
            var door = room.transform.Find("Door_W").GetComponent<Door>();
            door.gameObject.SetActive(true);
            door.direction = new Vector2Int(-1, 0);
            door.parentGrid = gridPos;
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
        }

        // Save room reference
        spawnedRooms[gridPos] = room;
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
