using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class JumperBehavior : EnemyBehavior, IEntityBehavior
{
    [Header("Jump Settings")]
    public float tickRate = 0.5f;           // Time between evaluating next jump
    public float jumpDuration = 0.3f;       // How long the jump takes
    public float maxJumpDistance = 5f;      // Max distance per jump
    [Range(0f, 1f)]
    public float playerBias = 0.1f;         // 0 = purely random, 1 = always toward player
    public Vector2 jumpCooldownRange = new Vector2(0.5f, 1.5f); // Random wait between jumps

    private GameObject player;
    private bool isJumping = false;
    private Vector3 jumpStartPos;
    private Vector3 jumpTarget;
    private float jumpTimer = 0f;
    private float cooldownTimer = 0f;
    private float currentJumpCooldown;

    public override void Initialize(EnemyController controller)
    {
        base.Initialize(controller);

        if (controller.health != null)
        {
            controller.health.OnDeath += OnDeath;
            controller.health.OnHit += OnHit;
        }

        player = GameObject.FindGameObjectWithTag("Player");

        // Initial cooldown
        currentJumpCooldown = Random.Range(jumpCooldownRange.x, jumpCooldownRange.y);
    }

    public void OnDeath()
    {
        Debug.Log(this.name + " died");
        controller.animator.SetTrigger("Die");
        StopAllCoroutines();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.white;
        GetComponent<Collider2D>().enabled = false;
        controller.StopUpdate();
        StartCoroutine(DelayedDeath());
    }

    private IEnumerator DelayedDeath()
    {
        // Wait for the death animation to finish (adjust time as needed)
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    public void OnHit()
    {
        controller.voice.Play();
        StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;
        sr.color = Color.lightPink; // Flash red on hit
        yield return new WaitForSeconds(0.1f); // Duration of the flash
        sr.color = originalColor; // Revert to original color
    }

    public override void OnUpdate()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
        }

        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            float t = jumpTimer / jumpDuration;

            controller.transform.position = Vector3.Lerp(jumpStartPos, jumpTarget, t);

            if (t >= 1f)
            {
                Debug.Log($"Jump complete! Start: {jumpStartPos}, Target: {jumpTarget}, Final: {controller.transform.position}");
                isJumping = false;
                controller.agent.SetDestination(controller.transform.position);
                cooldownTimer = 0f;
                currentJumpCooldown = Random.Range(jumpCooldownRange.x, jumpCooldownRange.y);
                jumpTimer = 0f;
            }
        }
        else
        {
            // Waiting between jumps
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= currentJumpCooldown)
            {
                StartJump();
            }
        }
    }

    private void StartJump()
    {
        // Find reachable nodes
        List<NavigationNode> candidates = new List<NavigationNode>();
        foreach (var node in controller.manager.nodes)
        {
            float distance = Vector3.Distance(node.transform.position, controller.transform.position);
            if (distance <= maxJumpDistance && HasLineOfSight(node.transform.position))
            {
                candidates.Add(node);
            }
        }

        if (candidates.Count == 0) return;

        // Pick weighted node
        NavigationNode selected = PickWeightedNode(candidates);

        // **Correctly set start and target positions**
        jumpStartPos = controller.transform.position; // <-- important
        jumpTarget = selected.transform.position;     // <-- important
        jumpTimer = 0f;                               // <-- reset timer

        controller.agent.SetDestination(jumpTarget);

        controller.animator.SetTrigger("Jump");

        isJumping = true; // start the jump
    }

    private NavigationNode PickWeightedNode(List<NavigationNode> candidates)
    {
        // Decide: biased jump or random jump?
        if (Random.value > playerBias)
        {
            // Random jump - pick any candidate with equal probability
            return candidates[Random.Range(0, candidates.Count)];
        }

        // Biased jump - weight by inverse distance to player
        float totalWeight = 0f;
        List<float> weights = new List<float>();

        foreach (var node in candidates)
        {
            float distToPlayer = Vector3.Distance(node.transform.position, player.transform.position);

            // Use inverse distance: closer nodes get higher weight
            // Add small epsilon to avoid division by zero
            float weight = 1f / (distToPlayer + 0.1f);

            weights.Add(weight);
            totalWeight += weight;
        }

        // Weighted random selection
        float r = Random.Range(0f, totalWeight);
        float accum = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            accum += weights[i];
            if (r <= accum)
            {
                return candidates[i];
            }
        }

        return candidates[candidates.Count - 1];
    }

    private bool HasLineOfSight(Vector3 target)
    {
        Vector2 dir = (target - controller.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(controller.transform.position, dir.normalized, dir.magnitude, LayerMask.GetMask("Obstacle"));
        return hit.collider == null;
    }
}
