using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Min(0f)] public float moveSpeed = 5f;
    public Rigidbody2D rb;             // auto-filled if left empty
    public Animator animator;          // optional
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;

    Vector2 movement;
    Vector2 lastAim = Vector2.up;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
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

    void Shoot()
    {
        if (!projectilePrefab || !firePoint) return;

        var proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        var projCol = proj.GetComponent<Collider2D>();
        var playerCol = GetComponent<Collider2D>();
        if (projCol && playerCol) Physics2D.IgnoreCollision(projCol, playerCol);

        proj.GetComponent<Projectile>()?.SetDirection(lastAim);
    }
}
