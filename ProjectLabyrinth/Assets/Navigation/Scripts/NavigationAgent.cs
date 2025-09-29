using System.Collections.Generic;
using UnityEngine;

public class NavigationAgent : MonoBehaviour
{
    [SerializeField]
    float speed = 3f;

    public Vector3 targetPosition;
    public NavigationNode currentNode;
    public List<NavigationNode> nodePath = new List<NavigationNode>();
    public NavigationPath path = new NavigationPath(new List<NavigationNode>());

    private NavigationNode lastVisited;

    private NavigationNode targetNode;
    
    public float maxSightRange = 10f;

    public LayerMask obstacleMask;

    private bool usePathfinding = true;

    private void Start()
    {
        targetPosition = transform.position;
        currentNode = NavigationManager.instance.FindNearestNode(targetPosition);
        lastVisited = null;
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
                // NavigationNode previousNode = targetNode;
                targetNode = NavigationManager.instance.FindNearestNode(targetPosition);

                // if (previousNode == targetNode && usePathfinding) { return; } // This is temporary. I need to create a proper path DataStructure to hold and properly update Navigation Node Paths.
                NavigationPath newPath = new NavigationPath(NavigationManager.instance.GeneratePath(currentNode, targetNode, currentNode == lastVisited));

                if (path.nodes != null && path.nodes.Count > 0) 
                {
                    path.MergeWith(newPath);
                }
                else
                {
                    path = newPath;
                }

                NavigationNode startNode = FindBestNode();
                int index = path.nodes.IndexOf(startNode);
                path.nodes = path.nodes.GetRange(index, path.nodes.Count - index); // Heuristic. Attempt to start from the furthest along visible node on the path.

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
        if (path != null && path.nodes != null && path.nodes.Count > 0 && usePathfinding)
        {
            if ( path.nodes.Count > 0)
            {
                int x = 0;
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(path.nodes[x].transform.position.x, path.nodes[x].transform.position.y, -2), speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, path.nodes[x].transform.position) < 0.1f)
                {
                    currentNode = path.nodes[x];
                    path.nodes.RemoveAt(x);
                    lastVisited = currentNode;
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
            currentNode = lastVisited = NavigationManager.instance.FindNearestNode(targetPosition);
        }

    }

    private bool hasLineOfSight(Vector3 target)
    {
        Vector3 diff = (target - transform.position);
        Vector3 dir = diff.normalized;

        float distance = diff.magnitude;
        return !Physics2D.Raycast(transform.position, dir, distance, obstacleMask);
    }

    private NavigationNode FindBestNode()
    {
        NavigationNode bestNode = path.nodes[0];
        foreach(NavigationNode node in path.nodes)
        {
            if (hasLineOfSight(node.transform.position))
            {
                bestNode = node;
            }
        }

        return bestNode;
    }
}
