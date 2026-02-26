using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script for performing the A* Pathfinding Algorithm
public class NavigationManager : MonoBehaviour
{
    //// Singleton Class
    //public static NavigationManager instance;

    public List<NavigationNode> nodes = new List<NavigationNode>();

    private void Start()
    {
        //instance = this;

        nodes.AddRange(GetComponentsInChildren<NavigationNode>(true));
    }

    public NavigationNode GetRandomNode()
    {
        if (nodes == null || nodes.Count == 0) return null;
        return nodes[Random.Range(0, nodes.Count)];
    }

    // Generates a path based using the A* Pathfinding Algorithm
    public List<NavigationNode> GeneratePath(NavigationNode start, NavigationNode end, bool skipFirstNode = false)
    {
        if (start == null || end == null) return null;

        List<NavigationNode> openSet = new List<NavigationNode>();
        // Use a separate list for the resulting path (avoid aliasing with openSet).
        List<NavigationNode> resultPath = null;

        // Reset node scores / cameFrom
        foreach (NavigationNode node in nodes)
        {
            if (node == null) continue;
            node.gScore = float.MaxValue;
            node.cameFrom = null;
            node.hScore = 0f;
        }

        start.gScore = 0;
        start.hScore = Vector2.Distance(start.transform.position, end.transform.position);

        openSet.Add(start);

        while (openSet.Count > 0)
        {
            int lowestF = 0;
            for (int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i].FScore() < openSet[lowestF].FScore())
                {
                    lowestF = i;
                }
            }

            NavigationNode currentNode = openSet[lowestF];
            openSet.RemoveAt(lowestF);

            if (currentNode == end)
            {
                // Build path by walking back through cameFrom
                resultPath = new List<NavigationNode>();
                NavigationNode temp = currentNode;
                resultPath.Add(temp);

                while (temp != start)
                {
                    temp = temp.cameFrom;
                    // Defensive: if there's a null cameFrom, path is invalid
                    if (temp == null) break;
                    resultPath.Add(temp);
                }

                resultPath.Reverse();
                if (skipFirstNode && resultPath.Count > 0) resultPath.RemoveAt(0);
                return resultPath;
            }

            // Expand neighbors
            foreach (NavigationNode connectedNode in currentNode.connections)
            {
                if (connectedNode == null) continue;

                float tentativeG = currentNode.gScore + Vector2.Distance(currentNode.transform.position, connectedNode.transform.position);

                if (tentativeG < connectedNode.gScore)
                {
                    connectedNode.cameFrom = currentNode;
                    connectedNode.gScore = tentativeG;
                    connectedNode.hScore = Vector2.Distance(connectedNode.transform.position, end.transform.position);

                    if (!openSet.Contains(connectedNode))
                    {
                        openSet.Add(connectedNode);
                    }
                }
            }
        }

        // No path found
        return null;
    }


    // Returns the closest node to a given position
    public NavigationNode FindNearestNode(Vector3 position)
    {
        float minDistance = float.MaxValue;
        NavigationNode closest = null;

        foreach (var node in nodes)
        {
            if (node == null) continue;               // <--- IMPORTANT FIX
            if (node.gameObject == null) continue;    // <--- DOUBLE SAFE
            if (!node) continue;

            float distanceSqaured = (node.transform.position - position).sqrMagnitude; // Use square magnitudes because I only care about relative distances, not absolute. Save on performance

            if (distanceSqaured < minDistance)
            {
                closest = node;
                minDistance = distanceSqaured;
            }
        }

        if (closest == null)
        {
            Debug.LogError("Error, Could not find Closest Navigation Node.");
        }

        return closest;
    }
}
