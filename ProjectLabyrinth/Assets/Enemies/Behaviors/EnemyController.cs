using UnityEngine;

[RequireComponent (typeof(NavigationAgent))]
[RequireComponent (typeof(Health))]
public class EnemyController : MonoBehaviour
{
    public EnemyBehavior behavior;
    public Animator animator;

    [HideInInspector] public Health health;
    [HideInInspector] public NavigationAgent agent;
    [HideInInspector] public NavigationManager manager;

    private bool alive = true;

    public void setNavigationManager(NavigationManager manager)
    {
        this.manager = manager;
    }

    public void Initialize(NavigationManager manager)
    {
        this.manager = manager;

        agent = GetComponent<NavigationAgent>();
        health = GetComponent<Health>();
        animator = GetComponent<Animator>();

        agent.manager = manager;
        agent.enabled = true;

        if (behavior != null)
        {
            behavior.Initialize(this);
        }
    }
    private void Awake()
    {
        agent = GetComponent<NavigationAgent>();
        health = GetComponent<Health>();
        animator = GetComponent<Animator>();

        agent.manager = manager;
        agent.enabled = true;

        if (behavior != null)
        {
            behavior.Initialize(this);
        }
    }

    private void Update()
    {
        if (alive)
            behavior?.OnUpdate();
    }

    public void StopUpdate()
    {
        agent.enabled = false;
        alive = false;
    }
}
