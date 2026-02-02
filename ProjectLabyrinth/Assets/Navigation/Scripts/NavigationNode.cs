using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationNode : MonoBehaviour
{
    public NavigationNode cameFrom;

    public Vector2 position;
    public List<NavigationNode> connections = new List<NavigationNode>();

    // A Node for the A* Pathfinding Algorithm
    public float gScore;
    public float hScore;

    private void Awake()
    {
        if (position == null)
        {
            position = transform.position;
        }
    }

    public float FScore()
    {
        return gScore + hScore;
    }

    // Tool for visualizing connections
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.1f);

        if (connections == null) return;

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
