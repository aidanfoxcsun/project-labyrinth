using UnityEngine;

/// <summary>
/// Top-down Knight controller that drives Animator parameters:
/// Float: MoveX, MoveY, Speed
/// Bool : Guard, Dead
/// Trigger: Attack, Bash, Hit
///
/// Also flips SpriteRenderer when facing left (FlipX).
/// Assumes 3-direction sprites (Down/Up/Side) and Left is handled by flip.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class KnightController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public bool allowMoveWhileGuarding = false;   // set true if you want guard-walk
    public bool allowMoveWhileAttacking = false;  // set true if you want attack-walk

    [Header("Input")]
    public KeyCode attackKey = KeyCode.Space;
    public KeyCode guardKey = KeyCode.LeftShift;  // hold
    public KeyCode bashKey = KeyCode.E;           // parry/bash

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private Vector2 moveInput;
    private Vector2 lastFacing = Vector2.down;

    // Optional: simple locks (keeps things consistent with one-shot animations)
    private bool isDead;

    // Animator parameter hashes (faster + avoids typos)
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private static readonly int GuardHash = Animator.StringToHash("Guard");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int BashHash = Animator.StringToHash("Bash");
    private static readonly int HitHash = Animator.StringToHash("Hit");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Top-down settings safety
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (isDead) return;

        // 1) Read inputs
        ReadMovementInput();
        bool guardHeld = Input.GetKey(guardKey);

        // 2) Face direction tracking (for blend trees + flipping)
        UpdateFacingFromInput();

        // 3) Apply flip for left
        ApplyFlipX();

        // 4) Animator parameters (movement + guard)
        animator.SetBool(GuardHash, guardHeld);

        // Movement locking rules (optional but recommended)
        bool canMove = true;

        if (guardHeld && !allowMoveWhileGuarding) canMove = false;
        // If you want strict attack locks, set allowMoveWhileAttacking false.
        // We can't perfectly know "in attack" without an extra bool, so we keep it simple:
        // you can add animation events to toggle a bool if needed.
        // For now, allowMoveWhileAttacking just disables movement on key down for this frame.
        if (Input.GetKeyDown(attackKey) && !allowMoveWhileAttacking) canMove = false;
        if (Input.GetKeyDown(bashKey) && !allowMoveWhileAttacking) canMove = false;

        Vector2 appliedMove = canMove ? moveInput : Vector2.zero;

        animator.SetFloat(MoveXHash, lastFacing.x);
        animator.SetFloat(MoveYHash, lastFacing.y);
        animator.SetFloat(SpeedHash, appliedMove.sqrMagnitude); // drives Idle vs Moving transitions

        // 5) Actions
        // Attack: block if guarding or dead
        if (Input.GetKeyDown(attackKey) && !guardHeld)
        {
            animator.SetTrigger(AttackHash);
        }

        // Bash/parry: usually only makes sense while guarding, but if you want it anytime, remove guardHeld check.
        if (Input.GetKeyDown(bashKey) && guardHeld)
        {
            animator.SetTrigger(BashHash);
        }

        // Store applied movement for FixedUpdate
        moveInput = appliedMove;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Rigidbody2D movement (smooth + collision friendly)
        Vector2 newPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    private void ReadMovementInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 v = new Vector2(x, y);

        // Normalize so diagonals aren't faster
        moveInput = v.sqrMagnitude > 1f ? v.normalized : v;
    }

    private void UpdateFacingFromInput()
    {
        // Only update facing when there is input (prevents snapping while idle)
        if (moveInput.sqrMagnitude > 0.0001f)
        {
            // If your animation set is 3-direction (Up/Down/Side),
            // we want pure cardinal facing for cleaner blends:
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                lastFacing = new Vector2(Mathf.Sign(moveInput.x), 0f);
            else
                lastFacing = new Vector2(0f, Mathf.Sign(moveInput.y));
        }
    }

    private void ApplyFlipX()
    {
        // Left when facing X < 0 (Side-left)
        // Right when facing X > 0
        if (lastFacing.x < 0f) sr.flipX = true;
        else if (lastFacing.x > 0f) sr.flipX = false;
        // If facing Up/Down, keep last flip (prevents flicker)
    }

    // -----------------------------
    // Public hooks for your gameplay
    // -----------------------------

    /// <summary>Call when the knight takes damage (triggers Knight_Hit state via Any State).</summary>
    public void TriggerHit()
    {
        if (isDead) return;
        animator.SetTrigger(HitHash);
    }

    /// <summary>Call to kill the knight (routes to Death via Any State).</summary>
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.SetBool(DeadHash, true);

        // Stop movement and optionally disable collider scripts, etc.
        moveInput = Vector2.zero;
        animator.SetFloat(SpeedHash, 0f);
    }

    /// <summary>Optional: set facing explicitly (useful for knockback / aiming).</summary>
    public void SetFacing(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        // snap to cardinal
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            lastFacing = new Vector2(Mathf.Sign(dir.x), 0f);
        else
            lastFacing = new Vector2(0f, Mathf.Sign(dir.y));

        ApplyFlipX();
        animator.SetFloat(MoveXHash, lastFacing.x);
        animator.SetFloat(MoveYHash, lastFacing.y);
    }
}