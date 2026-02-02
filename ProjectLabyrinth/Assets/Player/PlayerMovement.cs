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

    public GameObject aimDirection;

    public CameraFollow cam;

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
        // Updated Shooting Logic
        // Use Arrow keys, twin stick shooter style.
        // WASD for movement
        // Arrow Keys for Shooting

        //movement.x = Input.GetAxisRaw("Horizontal");
        //movement.y = Input.GetAxisRaw("Vertical");

        movement = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) movement.y += 1;
        if (Input.GetKey(KeyCode.S)) movement.y -= 1;
        if (Input.GetKey(KeyCode.A)) movement.x -= 1;
        if (Input.GetKey(KeyCode.D)) movement.x += 1;

        movement = movement.normalized;

        // Arrow key aiming only
        Vector2 aim = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow)) aim.y += 1;
        if (Input.GetKey(KeyCode.DownArrow)) aim.y -= 1;
        if (Input.GetKey(KeyCode.LeftArrow)) aim.x -= 1;
        if (Input.GetKey(KeyCode.RightArrow)) aim.x += 1;

        if (aim.sqrMagnitude > 0.01f)
            lastAim = aim.normalized;

        Vector2 pos = transform.position;

        aimDirection.transform.position = Vector2.Lerp(aimDirection.transform.position, 
            lastAim + pos, 
            Time.deltaTime * 50.0f);

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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Room"))
        {
            GameObject room = collision.gameObject;
            cam.SetTargetDestination(new Vector2(room.transform.position.x, room.transform.position.y));
        }
    }
}
