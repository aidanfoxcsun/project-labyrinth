using UnityEngine;

public class BossPortalOnDeath : MonoBehaviour
{
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    private bool spawned;

    private void OnEnable()
    {
        GetComponent<Health>().OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        GetComponent<Health>().OnDeath -= OnDeath;
    }

    private void OnDeath()
    {
        // Prevent spawning during editor stop / domain reload
        if (!Application.isPlaying) return;
        if (spawned) return;

        if (portalPrefab == null)
        {
            Debug.LogError("[BossPortalOnDeath] portalPrefab not assigned.", this);
            return;
        }

        spawned = true;
        Instantiate(portalPrefab, transform.position + spawnOffset, Quaternion.identity);
    }
}