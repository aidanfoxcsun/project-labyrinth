using UnityEngine;

// Warden is essentially a stationary turrent that will periodically fire at the player until killed.
public class WardenBehavior : EnemyBehavior
{
    public float fireInterval = 2f;

    private float timer = 0f;

    public GameObject projectilePrefab;
    private BossProjectile projectile;

    private GameObject player;

    public override void Initialize(EnemyController controller)
    {
        base.Initialize(controller);

        player = GameObject.FindGameObjectWithTag("Player");
    }

    public override void OnUpdate()
    {
        timer += Time.deltaTime;
        if(timer >= fireInterval)
        {
            timer = 0f;
            ShootAtPlayer();
        }
    }

    private void ShootAtPlayer()
    {
        Vector3 dir = (player.transform.position - controller.transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, controller.transform.position, Quaternion.LookRotation(Vector3.forward, dir));
        projectile = proj.GetComponent<BossProjectile>();
        projectile.direction = dir;
    }
}
