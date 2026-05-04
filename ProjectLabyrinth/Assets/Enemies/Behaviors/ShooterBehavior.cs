using UnityEngine;

public class ShooterBehavior : EnemyBehavior, IEntityBehavior
{
    public float tickRate = 0.2f;

    [Tooltip("Time taken between shots in seconds")]
    public float cooldownTime = 1.5f;

    // Line-of-sight settings
    [Tooltip("Maximum distance at which this enemy will consider the player for LOS/shooting.")]
    public float sightRange = 12f;

    [Tooltip("If true, the enemy will only consider LOS if the player is within sightRange. If false, LOS raycast is unlimited.")]
    public bool limitSightByRange = true;

    public GameObject projectilePrefab;
    private BossProjectile projectile;

    // Optional: a layer mask to restrict what the raycast can hit (walls, obstacles).
    // If left as Default (everything), the raycast will hit colliders in the scene and we check for the Player tag.
    public LayerMask blockingMask = Physics2D.DefaultRaycastLayers;

    private GameObject player;


    private float speed = 1.0f;

    private void Start()
    {
        if (controller.health == null)
        {
            Debug.Log("cannot find health component!");
        }
        controller.health.OnDeath += OnDeath;
        controller.health.OnHit += OnHit;

        speed = controller.agent.GetSpeed();

        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void OnDeath()
    {
        // This is where any special behaviors need to go.
        // Loot drops, animations, second phases, etc.
        Debug.Log(this.name + " died");
        Destroy(this.gameObject);
    }

    public void OnHit()
    {
        // This is where any special behaviors need to go.
        // Change in behavior, animations, etc.
        Debug.Log(this.name + " has been hit");
    }

    private float frameCounter = 0f;

    // Returns true when there is an unobstructed line from this enemy to the player.
    // Uses a 2D raycast and checks whether the first hit is the player.
    private bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;

        Vector2 origin = transform.position;
        Vector2 target = player.transform.position;
        Vector2 dir = (target - origin).normalized;

        float maxDistance = limitSightByRange ? Vector2.Distance(origin, target) : Mathf.Infinity;
        if (limitSightByRange && maxDistance > sightRange) return false;

        // Raycast against blockingMask. If the first hit is the player -> LOS.
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, maxDistance, blockingMask);

        // If the raycast hit something, check whether it is the player (tag-based).
        if (hit.collider.CompareTag("Player"))
        {
            Debug.DrawRay(origin, dir * maxDistance, Color.green, 0.1f);
            return true;
        }

        // Hit an obstacle before reaching player
        Debug.DrawRay(origin, dir * maxDistance, Color.red, 0.1f);
        return false;
    }

    private void ShootAtPlayer()
    {
        Vector3 dir = (player.transform.position - controller.transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, controller.transform.position, Quaternion.LookRotation(Vector3.forward, dir));
        projectile = proj.GetComponent<BossProjectile>();
        projectile.direction = dir;
        if (controller.animator != null)
            controller.animator.SetBool("isFiring", false);
    }

    private float cooldownCounter = 0.0f;

    public override void OnUpdate()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        // Attempt to get LOS. If we have LOS, stop moving (we could shoot here).
        bool hasLOS = HasLineOfSightToPlayer();

        if (frameCounter >= tickRate && player != null)
        {
            if (!hasLOS)
            {
                // No LOS -> navigate toward player's position (attempt to gain LOS)
                controller.agent.SetSpeed(speed);
                controller.agent.SetDestination(player.transform.position);
            }
            else
            {
                // Has LOS -> stop moving (set destination to current position)
                controller.agent.SetSpeed(0);
                controller.agent.SetDestination(transform.position);
                // TODO: trigger shooting/aiming behavior here (this method focuses on LOS only)
                if(cooldownCounter >= cooldownTime)
                {
                    ShootAtPlayer();
                    cooldownCounter = 0;
                }
                cooldownCounter += tickRate;
                Debug.Log(cooldownCounter);
            }

            frameCounter = 0f;
        }

        if (controller.animator != null)
        {
            Vector3 dir = controller.agent.GetCurrentDirection();

            if (dir.x < 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
            else if (dir.x > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }

            controller.animator.SetBool("isWalking", controller.agent.GetIsWalking());
            controller.animator.SetFloat("DirX", dir.x);
            controller.animator.SetFloat("DirY", dir.y);
        }

        frameCounter += Time.deltaTime;
    }
}
