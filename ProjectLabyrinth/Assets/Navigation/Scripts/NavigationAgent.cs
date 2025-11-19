using System.Collections.Generic;
using UnityEngine;

public class NavigationAgent : MonoBehaviour
{
    [SerializeField]
    private float speed = 3f;

    public Vector3 targetPosition;
    public NavigationNode currentNode;
    public List<NavigationNode> path = new List<NavigationNode>();

    private NavigationNode targetNode;

    public void SetSpeed(float f)
    {
        speed = f;
    }

    private void Start()
    {
        targetPosition = transform.position;
        targetNode = currentNode;
        currentNode = NavigationManager.instance.FindNearestNode(targetPosition);
    }

    public void SetDestination(Vector3 target)
    {
        targetPosition = target;

        currentNode = NavigationManager.instance.FindNearestNode(transform.position);
        targetNode = NavigationManager.instance.FindNearestNode(targetPosition);

        path = NavigationManager.instance.GeneratePath(currentNode, targetNode);
        
    }

    public Vector3 GetCurrentDirection()
    {
        if (currentNode == null || targetNode == null) return Vector3.zero;
        return (path[0].position - currentNode.position).normalized * speed;
    }

    public bool GetIsWalking()
    {
        bool walking = Vector3.Magnitude(transform.position - targetPosition) > 0.1; 
        return walking;
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
}
