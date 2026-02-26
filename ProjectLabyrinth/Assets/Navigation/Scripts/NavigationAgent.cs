using System.Collections;
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

    public NavigationManager manager;

    public void setManager(NavigationManager manager)
    {
        this.manager = manager;
    }

    public void SetSpeed(float f)
    {
        speed = f;
    }

    // Added getter for speed (used by behaviors)
    public float GetSpeed()
    {
        return speed;
    }

    private void Start()
    {
        StartCoroutine(InitAfterFrame());
    }

    private IEnumerator InitAfterFrame()
    {
        yield return null; // wait so NavigationManager is ready

        if (manager == null)
        {
            Debug.LogError($"{name}: NavigationManager not set!");
            yield break;
        }

        currentNode = manager.FindNearestNode(transform.position);

        if (currentNode == null)
        {
            Debug.LogError($"{name}: No navigation node near spawn position!");
            yield break;
        }

        targetNode = currentNode;
        targetPosition = transform.position;
    }

    public void SetDestination(Vector3 target)
    {
        targetPosition = target;

        if (manager == null)
        {
            Debug.LogWarning($"{name}: Cannot set destination, NavigationManager is null.");
            return;
        }

        currentNode = manager.FindNearestNode(transform.position);
        targetNode = manager.FindNearestNode(targetPosition);

        // Skip the first node so the path does not start with the current node (avoids attempting to move to the node you're already on)
        path = manager.GeneratePath(currentNode, targetNode, true);
    }

    public Vector3 GetCurrentDirection()
    {
        if (currentNode == null || path == null || path.Count == 0)
            return Vector3.zero;
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

    public void Stop()
    {
        // Clear the path so FollowPath() has nothing to loop through
        if (path != null)
        {
            path.Clear();
        }

        // Set targetPosition to current position to stop the fallback "MoveTowards"
        targetPosition = transform.position;

        // Optional: Reset current node to the closest one if you want to be precise
        if (manager != null)
        {
            currentNode = manager.FindNearestNode(transform.position);
        }
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
