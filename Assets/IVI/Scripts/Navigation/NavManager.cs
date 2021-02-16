using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class NavManager : MonoBehaviour
{
    public static NavManager        inst;
    public NavNode[]                allNodes;
    public NavEdge[]                allEdges;
    public INavigable[]             allAgents;
    public Dictionary<NavNode, int> node2Index;
    public Dictionary<NavNode, int> groupNode2Index;
    public bool[,]                  adjMatrix;

    public int[]    nodeOccupancy;
    public int[,]   edgeOccupancy;
    public int[]    nodeDesired;
    public int[,]   edgeDesired;
    public int[]    nodeDiff;
    public int[,]   edgeDiff_OBSOLETE;
    
    public Dictionary<INavigable, NavNode>  prevAgentNode;
    public Dictionary<INavigable, NavNode>  nextAgentNode;

    public GameObject nodePrefab;
    public GameObject edgePrefab;
    public GameObject groupNodePrefab;
    public GameObject nodesGO;
    public GameObject edgesGO;
    
    public const float NODE_RADIUS = 1;
    public const float EDGE_HEIGHT = 0.5f;
    public const float EDGE_WIDTH = 1f;
    [SerializeField]
    public static bool VISUALIZE = true;
    
    void Awake()
    {
        inst = this;
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(Run());
        }
    }

    void Update()
    {
        inst = this;
    }

    IEnumerator Run()
    {
        yield return null;

        allNodes = GameObject.FindObjectsOfType<NavNode>();
        allEdges = GameObject.FindObjectsOfType<NavEdge>();
        allAgents = GameObject.FindObjectsOfType<INavigable>();
        node2Index = new Dictionary<NavNode, int>();
        groupNode2Index = new Dictionary<NavNode, int>();
        adjMatrix = new bool[allNodes.Length, allNodes.Length];
        #region Initialize Graph and Agents

        for (int i = 0; i < allNodes.Length; i++)
        {
            node2Index.Add(allNodes[i], i);
        }
        foreach (var groupNode in GameObject.FindObjectsOfType<GroupNavNode>())
        {
            groupNode2Index.Add(groupNode, node2Index[groupNode]);
        }

        foreach (var edge in allEdges)
        {
            var a = node2Index[edge.node1];
            var b = node2Index[edge.node2];

            adjMatrix[a, b] = true;
            adjMatrix[b, a] = true;

            edge.node1.GetNeighbors().Add(edge.node2, edge);
            edge.node2.GetNeighbors().Add(edge.node1, edge);
        }

        #endregion

        nodeOccupancy = new int[allNodes.Length];
        edgeOccupancy = new int[allNodes.Length, allNodes.Length];
        prevAgentNode = new Dictionary<INavigable, NavNode>();
        nextAgentNode = new Dictionary<INavigable, NavNode>();
        #region Initialize Agent Occupancy

        foreach (var agent in allAgents)
        {
            var closestNode = allNodes.Aggregate((a, b) => Vector3.Distance(agent.transform.position, a.transform.position) < Vector3.Distance(agent.transform.position, b.transform.position) ? a : b);

            var ind = node2Index[closestNode];
            nodeOccupancy[ind]++;
            edgeOccupancy[ind, ind] += 2;
            prevAgentNode.Add(agent, closestNode);
            nextAgentNode.Add(agent, closestNode);
        }

        #endregion

        nodeDesired = new int[allNodes.Length];
        edgeDesired = new int[allNodes.Length, allNodes.Length];
        nodeDiff = new int[allNodes.Length];
        edgeDiff_OBSOLETE = new int[allNodes.Length, allNodes.Length];
        #region Initialize Desired Occupancy

        foreach (var node in allNodes)
        {
            if (node.GetType() == typeof(GroupNavNode))
            {
                var groupNode = (GroupNavNode)node;
                nodeDesired[node2Index[groupNode]] = groupNode.groupSize;
            }
        }
        foreach (var edge in allEdges)
        {
            var a = node2Index[edge.node1];
            var b = node2Index[edge.node2];

            //edgeDesired[a, b] = edge.size;
            //edgeDesired[b, a] = edge.size;

            var maxScale = 100;
            switch (edge.constraint)
            {
                case NavEdge.Constraint.NONE:
                    edgeDesired[a, b] = 1;
                    edgeDesired[b, a] = 1;

                    break;
                case NavEdge.Constraint.NO_FLOW:
                    edgeDesired[a, b] = maxScale;
                    edgeDesired[b, a] = maxScale;

                    break;
                case NavEdge.Constraint.HIGH_FLOW:
                    edgeDesired[a, b] = -1;
                    edgeDesired[b, a] = -1;

                    break;
                case NavEdge.Constraint.FORWARD_FLOW:
                    edgeDesired[a, b] = 1;
                    edgeDesired[b, a] = maxScale;

                    break;
                case NavEdge.Constraint.BACKWARD_FLOW:
                    edgeDesired[a, b] = maxScale;
                    edgeDesired[b, a] = 1;

                    break;
            }
        }

        #endregion

        #region Start Navigation

        foreach (var agent in allAgents)
        {
            yield return UpdateAgentGoal(agent);
        }

        #endregion

        yield break;
    }
    
    #region Public Functions

    public GameObject CreateNode(GameObject example)
    {
        var GO = GameObject.Instantiate(nodePrefab);
        GO.transform.position = example.transform.position;
        GO.transform.parent = nodesGO.transform;
        GO.name = "Node " + (GameObject.FindObjectsOfType<NavNode>().Length);
        GO.GetComponent<NavNode>().radius = NODE_RADIUS;

        return GO;
    }

    public GameObject CreateGroupNode(GameObject example)
    {
        var GO = GameObject.Instantiate(groupNodePrefab);
        GO.transform.position = example.transform.position;
        GO.transform.parent = nodesGO.transform;
        GO.name = "Group Node " + (GameObject.FindObjectsOfType<NavNode>().Length);
        GO.GetComponent<NavNode>().radius = NODE_RADIUS;

        return GO;
    }

    public NavEdge CreateEdge(NavNode node1, NavNode node2)
    {
        var GO = GameObject.Instantiate(edgePrefab);
        GO.transform.parent = edgesGO.transform;
        GO.name = "Edge " + (GameObject.FindObjectsOfType<NavEdge>().Length);
        var navEdge = GO.GetComponent<NavEdge>();
        navEdge.node1 = node1;
        navEdge.node2 = node2;
        navEdge.width = EDGE_WIDTH;

        return navEdge;
    }
    
    public IEnumerator UpdateAgentGoal(INavigable agent)
    {
        #region Compute Node and Edge Differences

        nodeDiff = new int[allNodes.Length];
        for (int i = 0; i < allNodes.Length; i++)
        {
            nodeDiff[i] = nodeDesired[i] - nodeOccupancy[i];
        }
        //edgeDiff_OBSOLETE = new int[allNodes.Length, allNodes.Length];
        //for (int i = 0; i < allNodes.Length; i++)
        //{
        //    for (int j = 0; j < allNodes.Length; j++)
        //    {
        //        var coeff = edgeDesired[i, j] == 0 ? 0 : 1;
        //        edgeDiff_OBSOLETE[i, j] = (edgeDesired[i, j] - edgeOccupancy[i, j]) * coeff;
        //        edgeDiff_OBSOLETE[j, i] = (edgeDesired[j, i] - edgeOccupancy[j, i]) * coeff;
        //    }
        //}

        #endregion

        #region Initialize Prev, Curr, and Next Nodes

        var prevNode = prevAgentNode[agent];
        var currNode = nextAgentNode[agent];
        var prevInd = node2Index[prevNode];
        var currInd = node2Index[currNode];

        var nextInd = Random.Range(0, allNodes.Length);
        var nextNode = allNodes[nextInd];
        while (nextInd == currInd || groupNode2Index.ContainsKey(nextNode))
        {
            nextInd = Random.Range(0, allNodes.Length);
            nextNode = allNodes[nextInd];
        }

        #endregion

        if (groupNode2Index.Count > 0)
        {
            var maxGroupNode = Extension.MaxBy(groupNode2Index, p => nodeDiff[p.Value]);
            if (nodeDiff[maxGroupNode.Value] > 0 && maxGroupNode.Value != currInd)
            {
                nextInd = maxGroupNode.Value;
                nextNode = maxGroupNode.Key;
            }
        }

        var path = Dijkstra(currNode, nextNode);
        nextNode = path[1];
        nextInd = node2Index[nextNode];

        #region Update Occupancy

        edgeOccupancy[prevInd, currInd]--;
        //edgeOccupancy[currInd, prevInd]--;
        edgeOccupancy[currInd, nextInd]++;
        //edgeOccupancy[nextInd, currInd]++;
        nodeOccupancy[currInd]--;
        nodeOccupancy[nextInd]++;

        prevAgentNode[agent] = currNode;
        nextAgentNode[agent] = nextNode;

        #endregion

        #region Start Behavior

        agent.InitDest(nextNode);
        //print(string.Join(",", nodeOccupancy) + "\t\t" + nodeOccupancy.Sum() + " " + edgeOccupancy.Cast<int>().Sum() / 2);

        #endregion

        yield break;
    }

    #endregion

    #region Utility Functions

    private int MaxIndex(int[] arr)
    {
        float max = float.MinValue;
        int max_index = -1;

        for (int i = 0; i < arr.Length; i++)
            if (arr[i] >= max)
            {
                max = arr[i];
                max_index = i;
            }

        return max_index;
    }
    
    private List<NavNode> Dijkstra(NavNode start, NavNode goal) //Agents still go back to prev
    {
        var closed = new bool[allNodes.Length];
        var dist = new float[allNodes.Length];
        var par = new int[allNodes.Length];

        for (int i = 0; i < dist.Length; i++)
            dist[i] = float.MaxValue;

        var startInd = node2Index[start];
        dist[startInd] = 0;
        par[startInd] = -1;
        for (int i = 0; i < dist.Length; i++)
        {
            int currInd = MinDistIndex(dist, closed);
            closed[currInd] = true;

            for (int j = 0; j < dist.Length; j++)
            {
                if (!closed[j] &&
                    adjMatrix[currInd, j])
                {
                    //var newDist = dist[currInd] + Vector3.Distance(allNodes[currInd].transform.position, allNodes[j].transform.position) * Mathf.Exp(-1 * edgeDiff[currInd, j]);
                    //var newDist = dist[currInd] + 1 * Mathf.Exp(-1 * edgeDiff[currInd, j]);

                    var newDist = dist[currInd] + Vector3.Distance(allNodes[currInd].transform.position, allNodes[j].transform.position) * edgeDesired[currInd, j];
                    //var newDist = dist[currInd] + 1 * edgeDesired[currInd, j];
                    if (dist[currInd] != float.MaxValue &&
                        newDist < dist[j])
                    {
                        dist[j] = newDist;
                        par[j] = currInd;
                    }
                }
            }
        }

        var path = new List<NavNode>();
        var tempInd = node2Index[goal];
        while (tempInd != -1)
        {
            path.Add(allNodes[tempInd]);

            tempInd = par[tempInd];
        }
        path.Reverse();

        return path;
    }

    private int MinDistIndex(float[] dist, bool[] closed)
    {
        float min = float.MaxValue;
        int min_index = -1;

        for (int i = 0; i < dist.Length; i++)
            if (closed[i] == false && dist[i] <= min)
            {
                min = dist[i];
                min_index = i;
            }

        return min_index;
    }

    #endregion
}

public static class Extension {

    public static TSource MaxBy<TSource, TProperty>(this IEnumerable<TSource> source, System.Func<TSource, TProperty> selector)
    {
        // check args        

        using (var iterator = source.GetEnumerator())
        {
            if (!iterator.MoveNext())
                throw new System.InvalidOperationException();

            var max = iterator.Current;
            var maxValue = selector(max);
            var comparer = Comparer<TProperty>.Default;

            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                var currentValue = selector(current);

                if (comparer.Compare(currentValue, maxValue) > 0)
                {
                    max = current;
                    maxValue = currentValue;
                }
            }

            return max;
        }
    }

}