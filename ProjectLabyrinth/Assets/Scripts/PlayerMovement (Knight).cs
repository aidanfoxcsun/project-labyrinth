using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMelee : MonoBehaviour
{
    [Header("Movement")]
    [Min(0f)] public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Animator animator;              // optional

    [Header("Sword / Slash")]
    public GameObject swordPrefab;         // square sword prefab
    public float attackOffset = 0.55f;     // distance of sword from player
    public float attackDuration = 0.18f;   // slash time
    public float attackCooldown = 0.35f;
    public float slashAngle = 110f;        // total arc
    public int swordDamage = 1;

    Vector2 movement;
    Vector2 lastAim = Vector2.up;
    float nextAttackTime;

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
        // input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        if (movement.sqrMagnitude > 0.0001f)
            lastAim = movement;

        // melee input (Z or Left Mouse)
        if ((Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0)) && Time.time >= nextAttackTime)
        {
            StartCoroutine(SlashArc());
            nextAttackTime = Time.time + attackCooldown;
        }

        if (animator)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    IEnumerator SlashArc()
    {
        if (!swordPrefab) yield break;

        // 1) Create a pivot at the player's position (swing center)
        GameObject pivot = new GameObject("SlashPivot");
        pivot.transform.position = transform.position;
        pivot.transform.SetParent(transform);   // follow player if he moves slightly

        // 2) Spawn the sword as a child of the pivot, offset to the side
        GameObject sword = Instantiate(swordPrefab, pivot.transform);
        sword.transform.localPosition = Vector3.right * attackOffset;

        // optional: set damage/owner
        var col = sword.GetComponent<Collider2D>();
        var rb2d = sword.GetComponent<Rigidbody2D>();
        var swordLogic = sword.GetComponent<Sword>();   // your hitbox script (optional)
        if (swordLogic) swordLogic.owner = gameObject;

        // ignore self-collision if both colliders exist
        var playerCol = GetComponent<Collider2D>();
        if (col && playerCol) Physics2D.IgnoreCollision(col, playerCol);

        // 3) Compute start/end angles based on the facing direction (lastAim)
        float baseAngle = Mathf.Atan2(lastAim.y, lastAim.x) * Mathf.Rad2Deg;
        float start = baseAngle - slashAngle * 0.5f;
        float end   = baseAngle + slashAngle * 0.5f;

        // start at the beginning of the arc
        pivot.transform.rotation = Quaternion.Euler(0, 0, start);

        if (animator) animator.SetTrigger("Melee");

        // 4) Rotate the pivot over time → sword travels a nice arc
        float t = 0f;
        while (t < attackDuration)
        {
            float z = Mathf.Lerp(start, end, t / attackDuration);
            pivot.transform.rotation = Quaternion.Euler(0, 0, z);
            t += Time.deltaTime;
            yield return null;
        }
        pivot.transform.rotation = Quaternion.Euler(0, 0, end);

        // cleanup
        Destroy(pivot);
    }

    // (optional) gizmo to preview attack radius & direction
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + (Vector3)(lastAim.normalized * attackOffset);
        Gizmos.DrawWireSphere(center, 0.1f);
    }
}