using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof(PlayerStats))]
[RequireComponent (typeof(Health))]
public class PlayerMovement : MonoBehaviour
{
    [Min(0f)] public float moveSpeed = 5f;
    public Rigidbody2D rb;             // auto-filled if left empty
    public Animator animator;          // optional
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;

    // Loading Stats
    public PlayerStats playerStats;

    Vector2 movement;
    Vector2 lastAim = Vector2.up;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        UpdateStats();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        if (movement.sqrMagnitude > 0.0001f) lastAim = movement;

        if (Input.GetKeyDown(KeyCode.Space)) Shoot();

        if (animator)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);
        }
    }

    void FixedUpdate()
    {
        // <-- This actually moves the player
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void UpdateStats()
    {
        // Apply stats
        moveSpeed = playerStats.speed;
        GetComponent<Health>().maxHP = playerStats.maxHP;
    }

    void Shoot()
    {
        if (!projectilePrefab || !firePoint) return;

        var proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        proj.GetComponent<Projectile>().lifetime = playerStats.range;
        proj.GetComponent<Hazard>().setDamage(playerStats.getDamage());

        var projCol = proj.GetComponent<Collider2D>();
        var playerCol = GetComponent<Collider2D>();
        if (projCol && playerCol) Physics2D.IgnoreCollision(projCol, playerCol);

        proj.GetComponent<Projectile>()?.SetDirection(lastAim);
    }
}
