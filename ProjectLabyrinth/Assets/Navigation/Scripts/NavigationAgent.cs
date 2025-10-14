using System.Collections.Generic;
using UnityEngine;

public class NavigationAgent : MonoBehaviour
{
    [SerializeField]
    float speed = 3f;

    public Vector3 targetPosition;
    public NavigationNode currentNode;
    public NavigationPath path = new NavigationPath(new List<NavigationNode>());

    private NavigationNode lastVisited;
    private NavigationNode targetNode;

    [Header("Vision Settings")]
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
        float distance = diff.magnitude;

        if (distance < maxSightRange)
        {
            if(!hasLineOfSight(target, false))
            {
                targetNode = NavigationManager.instance.FindNearestNode(targetPosition);
                NavigationPath newPath = NavigationManager.instance.GeneratePath(currentNode, targetNode, currentNode == lastVisited);

                if (newPath != null && newPath.nodes != null && newPath.nodes.Count > 0)
                {
                    if (path.nodes != null && path.nodes.Count > 0)
                        path.MergeWith(newPath);
                    else
                        path = newPath;

                    
                    // pick the furthest visible node along the path
                    NavigationNode startNode = FindBestNode();
                    int index = path.nodes.IndexOf(startNode);
                    if (index >= 0 && index < path.nodes.Count)
                        path.nodes = path.nodes.GetRange(index, path.nodes.Count - index);
                    

                    usePathfinding = true;
                }
                else
                {
                    // Path failed, just stop or fallback
                    usePathfinding = false;
                }
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
        if (hasLineOfSight(targetPosition, false))
        {
            usePathfinding = false;
        }

        if (path != null && path.nodes != null && path.nodes.Count > 0 && usePathfinding)
        {
            NavigationNode nextNode = path.nodes[0];

            if(!hasLineOfSight(nextNode.transform.position, true))
            {
                bool foundVisible = false;
                for(int i = 0; i < path.nodes.Count; i++)
                {
                    if (hasLineOfSight(path.nodes[i].transform.position, true))
                    {
                        path.nodes.RemoveRange(0, i);
                        foundVisible = true;
                        break;
                    }
                }
                if (!foundVisible)
                {
                    // No visible nodes, stop pathfinding
                    usePathfinding = false;
                    return;
                }

                nextNode = path.nodes[0];
            }

            Vector3 nextPos = new Vector3(nextNode.transform.position.x, nextNode.transform.position.y, -2);

            transform.position = Vector3.MoveTowards(transform.position, nextPos, speed * Time.deltaTime);

            // arrived at Node
            if (Vector2.Distance(transform.position, nextNode.transform.position) < 0.1f)
            {
                currentNode = nextNode;
                path.nodes.RemoveAt(0);
                lastVisited = currentNode;
            }
        }
        else
        {
            // direct movement
            Vector3 dest = new Vector3(targetPosition.x, targetPosition.y, -2);
            transform.position = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
            currentNode = lastVisited = NavigationManager.instance.FindNearestNode(targetPosition);
        }

    }

    private bool hasLineOfSight(Vector3 target, bool drawRays)
    {
        float offset = GetComponent<Collider2D>().bounds.extents.x * 0.8f;
        Vector3 perp = Vector3.Cross(Vector3.forward, (target - transform.position).normalized).normalized;

        Vector3[] origins = {
            transform.position,
            transform.position + perp * offset,
            transform.position - perp * offset
        };

        bool blocked = false;

        foreach (var origin in origins)
        {
            Vector3 diff = target - origin;
            float distance = diff.magnitude;
            Vector3 dir = diff.normalized;

            // Draw the ray in the Scene view
            Color rayColor = Color.green;

            if (Physics2D.Raycast(origin, dir, distance, obstacleMask))
            {
                rayColor = Color.red; // red if blocked
                blocked = true;
            }

            if(drawRays)
                Debug.DrawRay(origin, dir * distance, rayColor);
        }

        return !blocked;
    }

    private NavigationNode FindBestNode()
    {
        if (!usePathfinding || path == null || path.nodes == null || path.nodes.Count == 0)
            return currentNode;

        NavigationNode bestNode = path.nodes[0];
        foreach(NavigationNode node in path.nodes)
        {
            if (hasLineOfSight(node.transform.position, true))
            {
                bestNode = node;
            }
        }

        return bestNode;
    }
}
