using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NavManager : MonoBehaviour
{
    public static NavManager inst;
    public static HashSet<NavNode> allNodes = new HashSet<NavNode>();
    public static HashSet<NavEdge> allEdges = new HashSet<NavEdge>();

    public GameObject nodePrefab;
    public GameObject edgePrefab;
    public GameObject groupNodePrefab;

    public const float NODE_RADIUS = 1;
    public const float EDGE_HEIGHT = 0.5f;
    public const float EDGE_WIDTH = 2f;
    public const bool VISUALIZE = true;

    void Awake()
    {
        inst = this;
    }

    void Update()
    {
        if (Application.isEditor)
        {
            inst = this;
        }
    }
    
    #region Public Functions

    public GameObject CreateNode(GameObject example)
    {
        var GO = GameObject.Instantiate(nodePrefab);
        GO.transform.position = example.transform.position;
        GO.transform.parent = transform;
        GO.name = "Node " + (allNodes.Count + 1);

        return GO;
    }

    public GameObject CreateGroupNode(GameObject example)
    {
        var GO = GameObject.Instantiate(groupNodePrefab);
        GO.transform.position = example.transform.position;
        GO.transform.parent = transform;
        GO.name = "Group Node " + (allNodes.Count + 1);

        return GO;
    }

    public NavEdge CreateEdge(NavNode node1, NavNode node2)
    {
        var GO = GameObject.Instantiate(edgePrefab);
        GO.transform.parent = transform;
        GO.name = "Edge " + (allEdges.Count + 1);
        var navEdge = GO.GetComponent<NavEdge>();
        navEdge.node1 = node1;
        navEdge.node2 = node2;

        return navEdge;
    }

    public List<NavNode> SearchEdge(NavNode startNode, NavEdge goalEdge)
    {
        var closed = new HashSet<NavNode>();
        var parent = new Dictionary<NavNode, NavNode>();
        NavNode goalNode = null;

        var open = new Queue<NavNode>();
        open.Enqueue(startNode);
        parent.Add(startNode, null);
        while (open.Count > 0)
        {
            var curr = open.Dequeue();
            closed.Add(curr);

            foreach (var neighbor in curr.GetNeighbors())
            {
                if (!closed.Contains(neighbor.Key) || neighbor.Value == goalEdge)
                {
                    parent.Add(neighbor.Key, curr);
                    open.Enqueue(neighbor.Key);

                    if (neighbor.Value == goalEdge)
                    {
                        goalNode = neighbor.Key;
                        goto End;
                    }
                }
            }
        }
        End:;

        var temp = goalNode;
        var path = new List<NavNode>();
        while (parent[temp] != null)
        {
            path.Add(temp);

            temp = parent[temp];
        }
        path.Reverse();

        return path;
    }

    #endregion
}
