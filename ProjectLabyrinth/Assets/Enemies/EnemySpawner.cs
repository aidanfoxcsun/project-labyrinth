using UnityEngine;
using System.Collections.Generic;

public enum EnemyType
{
    Runner,
    Jumper,
    Ranged
}

[System.Serializable]
public class EnemyEntry
{
    public EnemyType type;
    public GameObject prefab;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Pool")]
    public List<EnemyEntry> enemies = new List<EnemyEntry>();

    [Header("Spawn Settings")]
    public EnemyType typeToSpawn;

    private void Start()
    {
        // Just for testing. This should NOT happen on start. It should be triggered by entering the room
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        // Filter the enemy list by type
        List<GameObject> possibleEnemies = new List<GameObject>();
        foreach (var entry in enemies)
        {
            if (entry.type == typeToSpawn)
                possibleEnemies.Add(entry.prefab);
        }

        if (possibleEnemies.Count == 0)
        {
            Debug.LogWarning($"No enemies of type {typeToSpawn} found in the list.");
            return;
        }


        Vector2 spawnPos = (Vector2)transform.position;
        GameObject prefab = possibleEnemies[Random.Range(0, possibleEnemies.Count)];
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}
