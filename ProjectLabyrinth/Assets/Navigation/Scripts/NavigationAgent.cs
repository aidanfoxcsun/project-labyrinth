using System.Collections.Generic;
using UnityEngine;

public class NavigationAgent : MonoBehaviour
{
    [SerializeField]
    float speed = 3f;

    public Vector3 targetPosition;
    public NavigationNode currentNode;
    public List<NavigationNode> path = new List<NavigationNode>();

    private NavigationNode targetNode;
    
    public float maxSightRange = 10f;

    public LayerMask obstacleMask;

    private bool usePathfinding = true;

    private void Start()
    {
        targetPosition = transform.position;
        currentNode = NavigationManager.instance.FindNearestNode(targetPosition);
    }

    public void SetDestination(Vector3 target)
    {
        targetPosition = target;

        currentNode = NavigationManager.instance.FindNearestNode(transform.position);

        Vector3 diff = (target - transform.position);
        Vector3 dir = diff.normalized;

        float distance = diff.magnitude;

        if (distance < maxSightRange)
        {

            Debug.DrawRay(transform.position, dir * distance, Color.red);
            if(Physics2D.Raycast(transform.position, dir, distance, obstacleMask))
            {
                NavigationNode previousNode = targetNode;
                targetNode = NavigationManager.instance.FindNearestNode(targetPosition);

                if (previousNode == targetNode && usePathfinding) { return; }

                path = NavigationManager.instance.GeneratePath(currentNode, targetNode);
                usePathfinding = true;
            }
            else
            {
                usePathfinding = false;
            }
        }
        else
        {
            // Out of range, idle
            usePathfinding = false;
            targetPosition = transform.position;
        }

        
    }

    private void Update()
    {
        FollowPath();
    }

    public void FollowPath()
    {
        if (usePathfinding)
        {
            if (path.Count > 0)
            {
                int x = 0;
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(path[x].transform.position.x, path[x].transform.position.y, -2), speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, path[x].transform.position) < 0.1f)
                {
                    currentNode = path[x];
                    path.RemoveAt(x);
                    if (hasLineOfSight(targetPosition))
                    {
                        usePathfinding=false;
                    }
                }
            }
            else
            {
                SetDestination(targetPosition);
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPosition.x, targetPosition.y, -2), speed * Time.deltaTime);
        }

    }

    private bool hasLineOfSight(Vector3 target)
    {
        Vector3 diff = (target - transform.position);
        Vector3 dir = diff.normalized;

        float distance = diff.magnitude;
        return !Physics2D.Raycast(transform.position, dir, distance, obstacleMask);
    }
}
