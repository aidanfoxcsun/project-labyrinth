using UnityEngine;

// Warden is essentially a stationary turrent that will periodically fire at the player until killed.
public class WardenBehavior : EnemyBehavior, IEntityBehavior
{
    public float fireInterval = 2f;

    private float timer = 0f;

    public GameObject projectilePrefab;
    private BossProjectile projectile;

    private GameObject player;

    public override void Initialize(EnemyController controller)
    {
        base.Initialize(controller);

        if (controller.health != null)
        {
            controller.health.OnDeath += OnDeath;
            controller.health.OnHit += OnHit;
        }

        player = GameObject.FindGameObjectWithTag("Player");
    }

    public override void OnUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= fireInterval - 0.4f) PlayFiringAnimation();
        if(timer >= fireInterval)
        {
            timer = 0f;
            ShootAtPlayer();
        }
    }

    private void PlayFiringAnimation()
    {
        controller.animator.SetBool("isFiring", true);
    }

    public void OnDeath()
    {
        Destroy(controller.gameObject);
    }

    public void OnHit()
    {
        // Optional reaction to being hit. Play hit animation or enter second phase at some point.
    }

    private void ShootAtPlayer()
    {
        Vector3 dir = (player.transform.position - controller.transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, controller.transform.position, Quaternion.LookRotation(Vector3.forward, dir));
        projectile = proj.GetComponent<BossProjectile>();
        projectile.direction = dir;
        controller.animator.SetBool("isFiring", false);
    }
}
