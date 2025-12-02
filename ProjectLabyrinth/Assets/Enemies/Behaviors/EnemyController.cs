using UnityEngine;

[RequireComponent (typeof(NavigationAgent))]
[RequireComponent (typeof(Health))]
public class EnemyController : MonoBehaviour
{
    public EnemyBehavior behavior;
    public Animator animator;

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

        if (animator != null)
        {
            Vector3 dir = agent.GetCurrentDirection();

            if (dir.x < 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
            else if (dir.x > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }

                animator.SetBool("isWalking", agent.GetIsWalking());
            animator.SetFloat("DirX", dir.x);
            animator.SetFloat("DirY", dir.y);
        }
    }
}
