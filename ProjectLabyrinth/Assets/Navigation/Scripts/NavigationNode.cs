using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationNode : MonoBehaviour
{
    public NavigationNode cameFrom;
    public List<NavigationNode> connections;

    // A Node for the A* Pathfinding Algorithm
    public float gScore;
    public float hScore;

    public float FScore()
    {
        return gScore + hScore;
    }

    // Tool for visualizing connections
    private void OnDrawGizmos()
    {

        if (connections.Count > 0)
        {
            foreach (var connection in connections)
            {
                if (connection.connections.Contains(this))
                {
                    Gizmos.color = Color.green; // Two-way connection
                    Gizmos.DrawLine(transform.position, connection.transform.position);
                }
                else
                {
                    Gizmos.color = Color.yellow; // One-way connection
                    Gizmos.DrawLine(transform.position, connection.transform.position);
                }
            }
        }
    }
}
