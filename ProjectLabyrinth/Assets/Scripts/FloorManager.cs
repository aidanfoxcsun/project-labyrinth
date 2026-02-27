using UnityEngine;

public class FloorManager : MonoBehaviour
{
    [SerializeField] private DungeonGenerator dungeonGenerator;

    [Header("Scaling")]
    [SerializeField] private int startingRooms = 12;
    [SerializeField] private int roomsPerFloor = 3;

    public int FloorIndex { get; private set; } = 0;

    private void Awake()
    {
        if (dungeonGenerator == null)
            dungeonGenerator = FindFirstObjectByType<DungeonGenerator>();
    }

    public void AdvanceFloor()
    {
        FloorIndex++;

        // Increase room count each floor
        dungeonGenerator.targetRooms = startingRooms + FloorIndex * roomsPerFloor;

        dungeonGenerator.GenerateNewFloor();

        MovePlayerToStart();
    }

    private void MovePlayerToStart()
    {
        var player = FindFirstObjectByType<PlayerMovement>();
        if (player == null) return;

        Vector3 spawn = dungeonGenerator.GetStartWorldPosition();

        player.rb.position = spawn;
        player.rb.velocity = Vector2.zero;

        if (player.cam != null)
            player.cam.SetTargetDestination(new Vector2(spawn.x, spawn.y));
    }
}