using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnightController : MonoBehaviour
{
    public float moveSpeed = 5f;

    Rigidbody2D rb;
    Animator anim;
    Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Movement input
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input = input.normalized;

        // Animation: movement
        anim.SetFloat("Speed", input.sqrMagnitude);

        // Attack (J)
        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.SetTrigger("Attack");
        }

        // Block (hold K)
        bool blocking = Input.GetKey(KeyCode.K);
        anim.SetBool("Block", blocking);

        // Test bash (L)
        if (Input.GetKeyDown(KeyCode.L))
        {
            anim.SetTrigger("Bash");
        }

        // Test hit (H)
        if (Input.GetKeyDown(KeyCode.H))
        {
            anim.SetTrigger("Hit");
        }

        // Test death (M)
        if (Input.GetKeyDown(KeyCode.M))
        {
            anim.SetBool("Dead", true);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * moveSpeed * Time.fixedDeltaTime);
    }
}