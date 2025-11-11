using UnityEngine;

[RequireComponent (typeof(NavigationAgent))]
[RequireComponent (typeof(Health))]
public class EnemyController : MonoBehaviour
{
    public EnemyBehavior behavior;

    [HideInInspector] public Health health;
    [HideInInspector] public NavigationAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavigationAgent>();
        health = GetComponent<Health>();

        if (behavior != null)
        {
            behavior.Initialize(this);
        }
    }

    private void Update()
    {
        behavior?.OnUpdate();
    }
}
