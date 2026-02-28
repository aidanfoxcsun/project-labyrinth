using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class ArcherController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Attack")]
    public string attackButton = "Fire1";   // Animator params: Attack (Trigger), Hold (Bool), Release (Trigger)
    public bool lockMovementWhileAttacking = false;

    [Header("Evasion")]
    public string evadeButton = "Jump";     // Space by default (old Input Manager)
    public float evadeSpeed = 7f;           // burst speed
    public float evadeDuration = 0.18f;     // seconds
    public float evadeCooldown = 0.35f;     // seconds
    public bool evadeCancelsAttack = true;
    public bool invincibleWhileEvading = true;

    [Header("Hit / Health")]
    public int maxHP = 3;
    public float hitStunDuration = 0.15f;   // how long to lock movement after hit (seconds)

    [Header("Death")]
    public float deathStopVelocity = 0f;

    private Animator anim;
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    private Vector2 moveInput;
    private Vector2 faceDir = Vector2.down;

    // Evasion runtime
    private bool isEvading;
    private float evadeTimer;
    private float evadeCooldownTimer;
    private Vector2 evadeDir;

    // Hit runtime
    private bool isHitStunned;
    private float hitStunTimer;

    // Death runtime
    private bool isDead;

    // Health
    private int currentHP;

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        currentHP = maxHP;
    }

    void Update()
    {
        // If dead, hard stop and do nothing else.
        if (isDead || anim.GetBool("IsDead"))
        {
            moveInput = Vector2.zero;
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // Keep idle params stable
            anim.SetFloat("Speed", 0f);
            anim.SetFloat("FaceX", faceDir.x);
            anim.SetFloat("FaceY", faceDir.y);
            return;
        }

        // Update hit stun timer (locks movement + optionally blocks attack/evade)
        if (isHitStunned)
        {
            hitStunTimer -= Time.deltaTime;
            if (hitStunTimer <= 0f)
                isHitStunned = false;
        }

        // Read movement input first (used for evade direction selection)
        if (!isEvading && !isHitStunned && !(lockMovementWhileAttacking && anim.GetBool("IsAttacking")))
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(x, y).normalized;

            // Update facing when moving
            if (moveInput.sqrMagnitude > 0.0001f)
                faceDir = moveInput;
        }
        else
        {
            // During evade / hitstun / (optional) attack lock, movement input is ignored
            moveInput = Vector2.zero;
        }

        // --- Evasion input (priority) ---
        evadeCooldownTimer -= Time.deltaTime;

        if (!isEvading && !isHitStunned && evadeCooldownTimer <= 0f && Input.GetButtonDown(evadeButton))
        {
            // Choose direction: movement if any, otherwise facing
            Vector2 dir = (moveInput.sqrMagnitude > 0.0001f) ? moveInput : faceDir;
            dir = QuantizeToCardinal(dir);

            StartEvade(dir);

            // If evade should override attack, skip attack input this frame
            if (evadeCancelsAttack)
            {
                PushAnimatorLocomotionParams();
                return;
            }
        }

        // --- Attack input (matches Animator params: Attack/Hold/Release) ---
        // Optional: block attacking while hit stunned or evading
        if (!isEvading && !isHitStunned)
        {
            bool pressed = Input.GetButtonDown(attackButton);
            bool held = Input.GetButton(attackButton);
            bool released = Input.GetButtonUp(attackButton);

            if (pressed)
            {
                anim.SetTrigger("Attack");
                anim.SetBool("Hold", true);
            }

            if (held)
                anim.SetBool("Hold", true);

            if (released)
            {
                anim.SetBool("Hold", false);
                anim.SetTrigger("Release");
            }
        }

        // Flip for Left/Right (your side sprites face RIGHT)
        if (Mathf.Abs(faceDir.x) > Mathf.Abs(faceDir.y))
        {
            if (faceDir.x < 0) sr.flipX = true;
            else if (faceDir.x > 0) sr.flipX = false;
        }

        PushAnimatorLocomotionParams();
    }

    void FixedUpdate()
    {
        if (isDead || anim.GetBool("IsDead"))
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        // Evade movement overrides everything
        if (isEvading)
        {
            Vector2 v = evadeDir * evadeSpeed;

            if (rb != null) rb.linearVelocity = v;
            else transform.position += (Vector3)(v * Time.fixedDeltaTime);

            evadeTimer -= Time.fixedDeltaTime;
            if (evadeTimer <= 0f)
                EndEvade();

            return;
        }

        // During hitstun, keep player stationary (feel free to change to "reduced control" instead)
        if (isHitStunned)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        // Normal movement
        Vector2 velocity = moveInput * moveSpeed;

        if (rb != null)
            rb.linearVelocity = velocity;   // Unity 6 uses linearVelocity
        else
            transform.position += (Vector3)(velocity * Time.fixedDeltaTime);
    }

    // =========================
    // Public Damage / Hit / Death API
    // =========================

    public void TakeDamage(int damage, Vector2 hitFromPosition)
    {
        if (isDead) return;
        if (invincibleWhileEvading && isEvading) return;

        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        TakeHit(hitFromPosition);
    }

    public void TakeHit(Vector2 hitFromPosition)
    {
        if (isDead) return;
        if (invincibleWhileEvading && isEvading) return;

        // Direction the hit should play (away from source)
        Vector2 dir = ((Vector2)transform.position - hitFromPosition);
        dir = QuantizeToCardinal(dir);

        // Drive hit blend tree direction (HitX/HitY)
        anim.SetFloat("HitX", dir.x);
        anim.SetFloat("HitY", dir.y);

        // Trigger hit animation
        anim.SetTrigger("Hit");

        // Hitstun (locks movement for a short time)
        isHitStunned = true;
        hitStunTimer = hitStunDuration;

        // Stop motion immediately
        moveInput = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop all motion
        moveInput = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero * deathStopVelocity;

        // Animator: Die trigger + IsDead bool
        anim.SetBool("IsDead", true);
        anim.SetTrigger("Die");

        // Optional: disable collider so you don't keep taking damage
        // var col = GetComponent<Collider2D>();
        // if (col) col.enabled = false;
    }

    // Optional: call from an Animation Event at the end of death animation
    public void OnDeathAnimationComplete()
    {
        Debug.Log("Game Over");
        // Hook your UI here (show Game Over panel, reload scene, etc.)
    }

    // =========================
    // Evasion
    // =========================

    private void StartEvade(Vector2 dir)
    {
        dir = QuantizeToCardinal(dir);

        isEvading = true;
        evadeTimer = evadeDuration;
        evadeCooldownTimer = evadeCooldown;

        evadeDir = dir.normalized;

        // Drive evade blend tree direction (EvadeX/EvadeY)
        anim.SetFloat("EvadeX", evadeDir.x);
        anim.SetFloat("EvadeY", evadeDir.y);

        // Update facing to evade direction so idle looks consistent after dodge
        faceDir = evadeDir;

        anim.SetTrigger("Evade");
        anim.SetBool("IsEvading", true); // you have this param in Animator
    }

    private void EndEvade()
    {
        isEvading = false;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsEvading", false);
    }

    // =========================
    // Helpers
    // =========================

    private Vector2 QuantizeToCardinal(Vector2 v)
    {
        if (v.sqrMagnitude < 0.0001f) return Vector2.down;

        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0);
        else
            return new Vector2(0, Mathf.Sign(v.y));
    }

    private void PushAnimatorLocomotionParams()
    {
        anim.SetFloat("FaceX", faceDir.x);
        anim.SetFloat("FaceY", faceDir.y);
        anim.SetFloat("Speed", moveInput.magnitude);
    }

    // =========================
    // (Optional) Animation Event hooks for Attack lock
    // Add Animation Events to your attack clips if you want these to work.
    // =========================
    public void AttackBegin() => anim.SetBool("IsAttacking", true);
    public void AttackEnd() => anim.SetBool("IsAttacking", false);
}