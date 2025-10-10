using UnityEngine;

public class RunnerBehavior : MonoBehaviour, IEntityBehavior
{
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath += OnDeath;
        health.OnHit += OnHit;
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
}
