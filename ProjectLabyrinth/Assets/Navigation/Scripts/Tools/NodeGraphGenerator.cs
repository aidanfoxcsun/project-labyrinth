using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGraphGenerator : MonoBehaviour
{
    [Header("Grid Bounds")]
    public Vector2 boundA;
    public Vector2 boundB;

    [Header("Grid Settings")]
    public float spacing = 1f; // Distance between nodes
    public float nodeRadius = 0.4f;
    public LayerMask obstacleMask;
    public bool connectDiagonals = true;

    [HideInInspector]
    public List<NavigationNode> nodes = new List<NavigationNode>();

    //void Start()
    //{
    //    StartCoroutine(GenerateNextFrame());
    //}

    //private IEnumerator GenerateNextFrame()
    //{
    //    yield return null;
    //    boundA += new Vector2(transform.position.x, transform.position.y);
    //    boundB += new Vector2(transform.position.x, transform.position.y);
    //    GenerateGraph();
    //}

    public void GenerateGraph()
    {
        ClearGraph();

        boundA += new Vector2(transform.position.x, transform.position.y);
        boundB += new Vector2(transform.position.x, transform.position.y);

        Vector2 min = Vector2.Min(boundA, boundB);
        Vector2 max = Vector2.Max(boundA, boundB);

        int countX = Mathf.CeilToInt((max.x - min.x) / spacing);
        int countY = Mathf.CeilToInt((max.y - min.y) / spacing);

        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                Vector2 worldPos = new Vector2(min.x + x * spacing, min.y + y * spacing);
                if(!Physics2D.OverlapCircle(worldPos, nodeRadius, obstacleMask))
                {
                    var nodeGO = new GameObject($"NavNode_{x}_{y}");
                    nodeGO.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
                    nodeGO.transform.parent = transform;

                    var node = nodeGO.AddComponent<NavigationNode>();
                    node.position = worldPos;
                    nodes.Add(node);
                }
            }
        }

        ConnectNeighbors();
    }

    public void ConnectNeighbors()
    {
        float maxDist = spacing * 1.5f;
        foreach(var node in nodes)
        {
            foreach(var other in nodes)
            {
                if (other == node) continue;
                if(Vector2.Distance(node.position, other.position) < maxDist)
                {
                    if (!connectDiagonals)
                    {
                        Vector3 diff = other.position - node.position;
                        if (Mathf.Abs(diff.x) > 0.1f && Mathf.Abs(diff.y) > 0.1f)
                            continue;
                    }

                    node.connections.Add(other);
                }
            }
        }
    }

    public void ClearGraph()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        nodes.Clear();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 bottomLeft = new Vector3(boundA.x, boundA.y, 0f);
        Vector3 topRight = new Vector3(boundB.x, boundB.y, 0f);
        Vector3 topLeft = new Vector3(boundA.x, boundB.y, 0f);
        Vector3 bottomRight = new Vector3(boundB.x, boundA.y, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
    }
#endif

}
