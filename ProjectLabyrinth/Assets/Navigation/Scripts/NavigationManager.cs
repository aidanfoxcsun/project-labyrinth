using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script for performing the A* Pathfinding Algorithm
public class NavigationManager : MonoBehaviour
{
    // Singleton Class
    public static NavigationManager instance;

    public List<NavigationNode> nodes = new List<NavigationNode>();

    private void Awake()
    {
        instance = this;

        foreach (NavigationNode node in FindObjectsByType<NavigationNode>(FindObjectsSortMode.None))
        {
            nodes.Add(node);
        }
    }

    // Generates a path based using the A* Pathfinding Algorithm
    public List<NavigationNode> GeneratePath(NavigationNode start, NavigationNode end, bool skipFirstNode = false)
    {
        List<NavigationNode> openSet = new List<NavigationNode>();
        List<NavigationNode> path = openSet;

        foreach(NavigationNode node in nodes)
        {
            node.gScore = float.MaxValue;
        }

        start.gScore = 0;
        start.hScore = Vector2.Distance(start.transform.position, end.transform.position);

        openSet.Add(start);

        while (openSet.Count > 0)
        {
            int lowestF = 0;
            for(int i = 0; i < openSet.Count; i++) 
            {
                if (openSet[i].FScore() < openSet[lowestF].FScore())
                {
                    lowestF = i;
                }
            }

            NavigationNode currentNode = openSet[lowestF];
            openSet.Remove(currentNode);

            if (currentNode == end)
            {
                path.Insert(0, end);


                while(currentNode != start)
                {
                    currentNode = currentNode.cameFrom;
                    path.Add(currentNode);
                }

                path.Reverse();
                if (skipFirstNode) path.RemoveAt(0);
                return path;
            }

            foreach(NavigationNode connectedNode in currentNode.connections)
            {
                float heldGScore = currentNode.gScore + Vector2.Distance(currentNode.transform.position, connectedNode.transform.position);

                if(heldGScore < connectedNode.gScore)
                {
                    connectedNode.cameFrom = currentNode;
                    connectedNode.gScore = heldGScore;
                    connectedNode.hScore = Vector2.Distance(connectedNode.transform.position, end.transform.position);

                    if (!openSet.Contains(connectedNode))
                    {
                        openSet.Add(connectedNode);
                    }
                }
            }
        }

        return null;
    }


    // Returns the closest node to a given position
    public NavigationNode FindNearestNode(Vector3 position)
    {
        float minDistance = float.MaxValue;
        NavigationNode closest = null;

        foreach(NavigationNode node in FindObjectsByType<NavigationNode>(FindObjectsSortMode.None))
        {
            float distanceSqaured = (node.transform.position - position).sqrMagnitude; // Use square magnitudes because I only care about relative distances, not absolute. Save on performance

            if(distanceSqaured < minDistance)
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
