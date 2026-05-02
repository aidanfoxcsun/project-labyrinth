using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Health))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public GameObject aimDirection;
    public CameraFollow cam;

    [Header("Stats (read from PlayerStats — do not set manually)")]
    [SerializeField] private float projectileSpeed = 10f;

    private PlayerStats playerStats;
    private Health health;

    private Vector2 movement;
    private Vector2 lastAim = Vector2.up;

    // Fire rate cooldown
    private float fireCooldown = 0f;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        health = GetComponent<Health>();

        if (!rb) rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        ApplyStats();
    }

    private void OnEnable() { health.OnHit += Health_OnHit; }
    private void OnDisable() { health.OnHit -= Health_OnHit; }

    private void Health_OnHit()
    {
        animator?.SetTrigger("Hit");
    }

    void Update()
    {
        HandleMovementInput();
        HandleAimInput();
        HandleFireInput();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * playerStats.speed * Time.fixedDeltaTime);
    }

    // =========================================================
    // Input
    // =========================================================
    private void HandleMovementInput()
    {
        movement = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) movement.y += 1;
        if (Input.GetKey(KeyCode.S)) movement.y -= 1;
        if (Input.GetKey(KeyCode.A)) movement.x -= 1;
        if (Input.GetKey(KeyCode.D)) movement.x += 1;
        movement = movement.normalized;
    }

    private void HandleAimInput()
    {
        Vector2 aim = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow)) aim.y += 1;
        if (Input.GetKey(KeyCode.DownArrow)) aim.y -= 1;
        if (Input.GetKey(KeyCode.LeftArrow)) aim.x -= 1;
        if (Input.GetKey(KeyCode.RightArrow)) aim.x += 1;

        if (aim.sqrMagnitude > 0.01f)
            lastAim = aim.normalized;

        aimDirection.transform.position = Vector2.Lerp(
            aimDirection.transform.position,
            lastAim + (Vector2)transform.position,
            Time.deltaTime * 50f);
    }

    private void HandleFireInput()
    {
        fireCooldown -= Time.deltaTime;

        // fireRate = shots per second, so cooldown = 1 / fireRate
        float cooldownTime = playerStats.fireRate > 0f ? 1f / playerStats.fireRate : 1f;

        if (Input.GetKey(KeyCode.Space) && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = cooldownTime;
        }
    }

    private void UpdateAnimator()
    {
        if (!animator) return;
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }

    // =========================================================
    // Shooting
    // =========================================================
    private void Shoot()
    {
        if (!projectilePrefab || !firePoint) return;

        if (animator)
        {
            animator.SetFloat("FaceX", lastAim.x);
            animator.SetFloat("FaceY", lastAim.y);
            animator.SetTrigger("Attack");
        }

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Apply stats to projectile
        proj.GetComponent<Projectile>().lifetime = playerStats.range;
        proj.GetComponent<Hazard>().setDamage(playerStats.getDamage());
        proj.GetComponent<Projectile>()?.SetDirection(lastAim);

        // Ignore collision between projectile and player
        Collider2D projCol = proj.GetComponent<Collider2D>();
        Collider2D playerCol = GetComponent<Collider2D>();
        if (projCol && playerCol)
            Physics2D.IgnoreCollision(projCol, playerCol);
    }

    // =========================================================
    // Stat Sync
    // =========================================================

    // Call this after any stat change to re-sync everything
    public void ApplyStats()
    {
        // Speed is read directly in FixedUpdate from playerStats.speed
        // so nothing to cache there — always live

        // Sync Health component with PlayerStats
        health.maxHP = playerStats.maxHP;
        health.hitPoints = Mathf.Min(health.hitPoints, health.maxHP);

        // Reset fire cooldown in case fireRate changed significantly
        fireCooldown = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Room"))
            cam.SetTargetDestination(collision.transform.position);
    }
}