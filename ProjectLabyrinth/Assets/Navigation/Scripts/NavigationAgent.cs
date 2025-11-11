using System.Collections.Generic;
using UnityEngine;

public class NavigationAgent : MonoBehaviour
{
    [SerializeField]
    private float speed = 3f;

    public Vector3 targetPosition;
    public NavigationNode currentNode;
    public List<NavigationNode> path = new List<NavigationNode>();

    public void SetSpeed(float f)
    {
        speed = f;
    }

    private void Start()
    {
        targetPosition = transform.position;
        currentNode = NavigationManager.instance.FindNearestNode(targetPosition);
    }

    public void SetDestination(Vector3 target)
    {
        targetPosition = target;

        currentNode = NavigationManager.instance.FindNearestNode(transform.position);
        NavigationNode targetNode = NavigationManager.instance.FindNearestNode(targetPosition);

        path = NavigationManager.instance.GeneratePath(currentNode, targetNode);

        //if (distance < maxSightRange)
        //{
        //    if(!hasLineOfSight(target, false))
        //    {
        //        targetNode = NavigationManager.instance.FindNearestNode(targetPosition);
        //        NavigationPath newPath = NavigationManager.instance.GeneratePath(currentNode, targetNode, currentNode == lastVisited);

        //        if (newPath != null && newPath.nodes != null && newPath.nodes.Count > 0)
        //        {
        //            if (path.nodes != null && path.nodes.Count > 0)
        //                path.MergeWith(newPath);
        //            else
        //                path = newPath;

                    
        //            // pick the furthest visible node along the path
        //            NavigationNode startNode = FindBestNode();
        //            int index = path.nodes.IndexOf(startNode);
        //            if (index >= 0 && index < path.nodes.Count)
        //                path.nodes = path.nodes.GetRange(index, path.nodes.Count - index);
                    

        //            usePathfinding = true;
        //        }
        //        else
        //        {
        //            // Path failed, just stop or fallback
        //            usePathfinding = false;
        //        }
        //    }
        //    else
        //    {
        //        usePathfinding = false;
        //    }
        //}
        //else
        //{
        //    // Out of range, idle
        //    usePathfinding = false;
        //    targetPosition = transform.position;
        //}

        
    }

    private void Update()
    {
        FollowPath();
    }

    public void FollowPath()
    {
        if (path == null || path.Count == 0)
        {
            // No path, move directly towards target (optional)
            Vector3 dest = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
            return;
        }

        // Move towards the first node in the path
        NavigationNode nextNode = path[0];
        Vector3 nextPos = new Vector3(nextNode.transform.position.x, nextNode.transform.position.y, transform.position.z);

        transform.position = Vector3.MoveTowards(transform.position, nextPos, speed * Time.deltaTime);

        // Arrived at node
        if (Vector2.Distance(transform.position, nextPos) < 0.1f)
        {
            currentNode = nextNode;
            path.RemoveAt(0);
        }
    }

    //private bool hasLineOfSight(Vector3 target, bool drawRays)
    //{
    //    float offset = GetComponent<Collider2D>().bounds.extents.x * 0.8f;
    //    Vector3 perp = Vector3.Cross(Vector3.forward, (target - transform.position).normalized).normalized;

    //    Vector3[] origins = {
    //        transform.position,
    //        transform.position + perp * offset,
    //        transform.position - perp * offset
    //    };

    //    bool blocked = false;

    //    foreach (var origin in origins)
    //    {
    //        Vector3 diff = target - origin;
    //        float distance = diff.magnitude;
    //        Vector3 dir = diff.normalized;

    //        // Draw the ray in the Scene view
    //        Color rayColor = Color.green;

    //        if (Physics2D.Raycast(origin, dir, distance, obstacleMask))
    //        {
    //            rayColor = Color.red; // red if blocked
    //            blocked = true;
    //        }

    //        if(drawRays)
    //            Debug.DrawRay(origin, dir * distance, rayColor);
    //    }

    //    return !blocked;
    //}

    //private NavigationNode FindBestNode()
    //{
    //    if (!usePathfinding || path == null || path.nodes == null || path.nodes.Count == 0)
    //        return currentNode;

    //    NavigationNode bestNode = path.nodes[0];
    //    foreach(NavigationNode node in path.nodes)
    //    {
    //        if (hasLineOfSight(node.transform.position, true))
    //        {
    //            bestNode = node;
    //        }
    //    }

    //    return bestNode;
    //}
}
