using System.Collections.Generic;
using UnityEngine;

public class NPC_Controller : MonoBehaviour
{
    public NavigationNode currentNode;
    public List<NavigationNode> path = new List<NavigationNode>();

    private void Update()
    {
        CreatePath();
    }

    public void CreatePath()
    {
        if (path.Count > 0)
        {
            int x = 0;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(path[x].transform.position.x, path[x].transform.position.y, -2), 3 * Time.deltaTime);

            if (Vector2.Distance(transform.position, path[x].transform.position) < 0.1f)
            {
                currentNode = path[x];
                path.RemoveAt(x);
                Debug.Log(path);
            }
        }
        else
        {
            NavigationNode[] nodes = FindObjectsByType<NavigationNode>(FindObjectsSortMode.None);
            while (path == null || path.Count == 0)
            {
                path = NavigationManager.instance.GeneratePath(currentNode, nodes[Random.Range(0, nodes.Length)], false);
            }
        }
    }

}
