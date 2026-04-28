using UnityEngine;

// Drop this on any boss GameObject alongside its Health component.
// It handles all HUD communication — no changes needed to behavior scripts.
[RequireComponent(typeof(Health))]
public class BossHUDBridge : MonoBehaviour
{
    [Header("Display")]
    public string bossDisplayName = "Boss";

    private Health health;
    private HUDController hud;

    void Awake()
    {
        health = GetComponent<Health>();
        hud = FindObjectOfType<HUDController>();
    }

    void Start()
    {
        // Show the bar as soon as the boss spawns
        hud?.ShowBoss(bossDisplayName, health.maxHP);

        // Hook into Health events — no behavior script needs to know about the HUD
        health.OnHit += HandleHit;
        health.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        // Always unsubscribe to avoid ghost callbacks
        if (health == null) return;
        health.OnHit -= HandleHit;
        health.OnDeath -= HandleDeath;
    }

    private void HandleHit()
    {
        hud?.SetBossHP(health.hitPoints);
    }

    private void HandleDeath()
    {
        hud?.HideBoss();
    }
}