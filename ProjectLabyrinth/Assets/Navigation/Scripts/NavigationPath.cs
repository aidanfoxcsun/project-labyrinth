using UnityEngine;
using System.Collections.Generic;

public class NavigationPath
{
    public List<NavigationNode> nodes;

    public NavigationPath(List<NavigationNode> nodes)
    {
        this.nodes = new List<NavigationNode>(nodes);
    }

    public void MergeWith(NavigationPath other)
    {
        if (other == null || other.nodes == null || other.nodes.Count == 0)
            return;

        if (nodes == null || nodes.Count == 0)
        {
            nodes = new List<NavigationNode>(other.nodes);
            return;
        }

        int minCount = Mathf.Min(nodes.Count, other.nodes.Count);

        for (int i = 0; i < minCount; i++)
        {
            if (nodes[i] == other.nodes[i]) // if the lists are indentical up to this point, skip node
            {
                continue;
            }

            nodes.RemoveRange(i, nodes.Count - i); // Remove the tail of the list

            nodes.AddRange(other.nodes.GetRange(i, nodes.Count - i)); // Add them to the current list
            break;
        }

        // if we got here and lists are identical up to minCount but lengths differ
        if (other.nodes.Count > nodes.Count)
        {
            nodes.AddRange(other.nodes.GetRange(nodes.Count, other.nodes.Count - nodes.Count));
        }
    }
}
