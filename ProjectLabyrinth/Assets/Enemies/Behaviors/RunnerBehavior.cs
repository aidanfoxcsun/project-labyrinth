using UnityEngine;

public class RunnerBehavior : EnemyBehavior, IEntityBehavior
{
    public float tickRate = 0.2f;

    private GameObject player;

    private void Start()
    {
        controller.health.OnDeath += OnDeath;
        controller.health.OnHit += OnHit;

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

    public override void OnUpdate()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (frameCounter >= tickRate && player != null)
        {
            controller.agent.SetDestination(player.transform.position);
            frameCounter = 0f;
        }

        frameCounter += Time.deltaTime;
    }
}
