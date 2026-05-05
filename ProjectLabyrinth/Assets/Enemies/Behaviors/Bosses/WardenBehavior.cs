using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Warden is essentially a stationary turret that will periodically fire at the player until killed.
// It also periodically summons minion enemies at designated spawn points.
public class WardenBehavior : EnemyBehavior, IEntityBehavior
{
    [Header("Firing")]
    public float fireInterval = 2f;
    public GameObject projectilePrefab;

    [Header("Summoning")]
    public List<GameObject> spawnPoints;
    public GameObject minionPrefab;
    public float summonInterval = 8f;
    public int minionsPerSummon = 2;

    private float fireTimer = 0f;
    private float summonTimer = 0f;
    private bool isSummoning = false;

    private BossProjectile projectile;
    private GameObject player;

    public override void Initialize(EnemyController controller)
    {
        base.Initialize(controller);
        if (controller.health != null)
        {
            controller.health.OnDeath += OnDeath;
            controller.health.OnHit += OnHit;
        }
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public override void OnUpdate()
    {
        // Don't act while summoning
        if (isSummoning) return;

        fireTimer += Time.deltaTime;
        summonTimer += Time.deltaTime;

        // Telegraph firing animation
        if (fireTimer >= fireInterval - 0.4f)
            PlayFiringAnimation();

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            ShootAtPlayer();
        }

        // Summon takes priority over firing when its interval is reached
        if (summonTimer >= summonInterval)
        {
            summonTimer = 0f;
            fireTimer = 0f; // Reset fire timer so we don't immediately fire after summoning
            StartCoroutine(SummonMinions());
        }
    }

    private void PlayFiringAnimation()
    {
        controller.animator.SetBool("isFiring", true);
    }

    private IEnumerator SummonMinions()
    {
        isSummoning = true;

        controller.animator.SetBool("isFiring", false);
        controller.animator.SetTrigger("Summon");

        // Wait for the summon telegraph animation to play before spawning
        yield return new WaitForSeconds(0.6f);

        // Pick random unique spawn points up to minionsPerSummon
        List<GameObject> availablePoints = new List<GameObject>(spawnPoints);
        int spawnCount = Mathf.Min(minionsPerSummon, availablePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int index = Random.Range(0, availablePoints.Count);
            GameObject point = availablePoints[index];
            availablePoints.RemoveAt(index); // Avoid duplicate spawn points

            GameObject minion = Instantiate(minionPrefab, point.transform.position, Quaternion.identity);
            minion.GetComponent<EnemyController>().Initialize(controller.manager); 
        }

        isSummoning = false;
    }

    private void ShootAtPlayer()
    {
        Vector3 dir = (player.transform.position - controller.transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, controller.transform.position, Quaternion.LookRotation(Vector3.forward, dir));
        projectile = proj.GetComponent<BossProjectile>();
        projectile.direction = dir;
        controller.animator.SetBool("isFiring", false);
    }

    public void OnDeath()
    {
        controller.animator.SetTrigger("Die");
        StopAllCoroutines();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.white;
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(2f);
        Destroy(controller.gameObject);
    }

    public void OnHit()
    {
        StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;
        sr.color = new Color(1f, 0.75f, 0.8f); // Light pink — Color.lightPink doesn't exist in Unity
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
    }
}