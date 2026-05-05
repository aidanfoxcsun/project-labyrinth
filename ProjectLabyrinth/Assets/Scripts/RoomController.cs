using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public List<Door> doors = new List<Door>();
    public List<GameObject> enemies = new List<GameObject>();

    public NavigationManager navigationManager;

    private bool activated = false;
    private bool locked = false;

    public DungeonGenerator.RoomType roomType;

    void Start()
    {
        // collect doors
        doors.AddRange(GetComponentsInChildren<Door>(true));
    }

    public void SetRoomType(DungeonGenerator.RoomType type)
    {
        roomType = type;
    }

    public void RegisterEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
        enemy.SetActive(false); // off until player enters
        enemy.GetComponent<NavigationAgent>().setManager(navigationManager);
        if (enemy.GetComponent<EnemyController>() != null)
        {
            enemy.GetComponent<EnemyController>().setNavigationManager(navigationManager);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            switch (roomType)
            {
                case DungeonGenerator.RoomType.Normal:
                case DungeonGenerator.RoomType.Start:
                    AudioController.Instance.EnterNormalArea();
                    break;
                case DungeonGenerator.RoomType.Upgrade:
                    AudioController.Instance.EnterShop();
                    break;
                case DungeonGenerator.RoomType.Treasure:
                    AudioController.Instance.EnterItemRoom();
                    break;
                case DungeonGenerator.RoomType.Boss:
                    AudioController.Instance.EnterBossRoom();
                    break;
                default:
                    AudioController.Instance.EnterNormalArea();
                    break;
            }
        }

        if (!activated && other.CompareTag("Player"))
        {
            activated = true;
            ActivateRoom();
        }
    }

    private void ActivateRoom()
    {
        // enable enemies
        foreach (var e in enemies)
            if (e != null)
                e.SetActive(true);

        LockDoors();
    }

    private void LockDoors()
    {
        locked = true;

        foreach (Door d in doors)
            d.SetLocked(true);     // <--- use the correct method now
    }

    private void UnlockDoors()
    {
        locked = false;

        foreach (Door d in doors)
            d.SetLocked(false);    // <--- correct API
    }

    void Update()
    {
        if (!locked) return;

        // if ANY enemy exists, room is NOT cleared
        bool allDead = true;

        foreach (var e in enemies)
            if (e != null)
            {
                allDead = false;
                break;
            }

        if (allDead)
            UnlockDoors();
    }
}
