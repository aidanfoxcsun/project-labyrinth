using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MinotaurBehavior : EnemyBehavior, IEntityBehavior
{
    public enum MinotaurState { Patrolling, Telegraphing, Charging, Cooldown }
    public MinotaurState currentState = MinotaurState.Patrolling;

    [Header("Patrol Settings")]
    public float patrolSpeed = 3.5f;
    public float patrolRadius = 10f;
    private Vector3 patrolTarget;

    [Header("Charge Settings")]
    public float chargeSpeed = 15.0f;
    public float detectionRange = 8f;

    private GameObject player;
    private float stateTimer = 0f;

    public override void Initialize(EnemyController controller)
    {
        base.Initialize(controller);
        if (controller.health != null)
        {
            controller.health.OnDeath += OnDeath;
            controller.health.OnHit += OnHit;
        }

        player = GameObject.FindGameObjectWithTag("Player");
        SetNewPatrolPoint();
    }

    // Core Idea: Minotaur is a melee boss which will charge at the player in a
    // cardinal direction if it has line of sight.
    // Minotaur will have a short windup before charging, and a cooldown after the charge.
    private float currentAttackInterval = 0.0f;
    bool canCharge = false;
    public override void OnUpdate()
    {
        switch (currentState)
        {
            case MinotaurState.Patrolling:
                UpdatePatrol();
                CheckForPlayer();
                break;
            case MinotaurState.Telegraphing:
                // Waiting for Coroutine to finish
                break;
            case MinotaurState.Charging:
                // Handled by Coroutine
                break;
            case MinotaurState.Cooldown:
                UpdateCooldown();
                break;
        }
    }

    private void UpdatePatrol()
    {
        controller.agent.SetSpeed(patrolSpeed);

        // If reached patrol point, pick a new one
        if (Vector3.Distance(transform.position, patrolTarget) < 1f)
        {
            SetNewPatrolPoint();
        }
        controller.agent.SetDestination(patrolTarget);
    }

    private void SetNewPatrolPoint()
    {
        // Instead of random math, ask the manager for a real location
        NavigationNode randomNode = controller.agent.manager.GetRandomNode();

        if (randomNode != null)
        {
            patrolTarget = randomNode.transform.position;
            controller.agent.SetDestination(patrolTarget);
        }
    }

    private void CheckForPlayer()
    {
        if (player == null) return;

        Vector3 dirToPlayer = player.transform.position - transform.position;
        float dist = dirToPlayer.magnitude;

        // Only trigger charge if in range and aligned cardinally (your original logic)
        if (dirToPlayer.magnitude < detectionRange)
        {
            if (IsPlayerInCardinalLine(dirToPlayer.normalized))
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer.normalized, dist, LayerMask.GetMask("Obstacle"));

                if (hit.collider == null)
                {
                    // Path is clear! No walls in the way.
                    StartCoroutine(ChargeSequence());
                }
            }
        }
    }

    private bool IsPlayerInCardinalLine(Vector3 dir)
    {
        return (Mathf.Abs(dir.x) > 0.8f && Mathf.Abs(dir.y) < 0.2f) ||
               (Mathf.Abs(dir.y) > 0.8f && Mathf.Abs(dir.x) < 0.2f);
    }

    private IEnumerator ChargeSequence()
    {
        currentState = MinotaurState.Telegraphing;
        controller.agent.Stop(); // Stop NavMesh/Agent movement

        // 1. Windup
        yield return new WaitForSeconds(0.8f);

        // 2. Charge
        currentState = MinotaurState.Charging;
        Vector3 chargeDir = GetCardinalDirection(player.transform.position);

        float chargeTime = 1.5f;
        while (chargeTime > 0)
        {
            // Manual movement during charge (ignoring pathfinding)
            transform.position += chargeDir * chargeSpeed * Time.deltaTime;
            chargeTime -= Time.deltaTime;
            yield return null;
        }

        // 3. Cooldown
        currentState = MinotaurState.Cooldown;
        stateTimer = 2.0f; // 2 seconds of being "tired"
    }

    private void UpdateCooldown()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0) currentState = MinotaurState.Patrolling;
    }

    private Vector3 GetCardinalDirection(Vector3 pos)
    {
        Vector3 dir = pos - controller.transform.position;
        if(Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return new Vector3(Mathf.Sign(dir.x), 0, 0);
        }
        else
        {
            return new Vector3(0, Mathf.Sign(dir.y), 0);
        }
    }

    public void OnDeath()
    {
        Destroy(controller.gameObject);
    }

    public void OnHit()
    {
        // Optional reaction to being hit. Play hit animation or enter second phase at some point.
    }
}
