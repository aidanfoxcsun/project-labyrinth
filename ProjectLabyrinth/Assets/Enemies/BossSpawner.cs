using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public GameObject bossPrefab;

    public void SpawnBoss(Transform roomTransform)
    {
        RoomController roomCtrl = roomTransform.GetComponent<RoomController>();

        Vector2 spawnPos = (Vector2)transform.position;
        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        if (roomCtrl != null)
            roomCtrl.RegisterEnemy(boss);
    }
}
