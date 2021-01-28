using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class NavNode : MonoBehaviour
{
    public bool createNode = false;
    public bool createGroupNode = false;
    public GameObject createConnection = null;
    public float radius;

    private Dictionary<NavNode, NavEdge> neighbors = new Dictionary<NavNode, NavEdge>();
    private MeshRenderer render;

    public void Start()
    {
        render = GetComponent<MeshRenderer>();
        NavManager.allNodes.Add(this);

        radius = NavManager.NODE_RADIUS;
    }

    public void Update()
    {
        if (Application.isEditor)
        {
            #region Editor

            if (render == null)
                render = GetComponent<MeshRenderer>();

            render.enabled = true;
            transform.localScale = Vector3.one * radius * 2;

            NavManager.allNodes.Add(this);

            if (createNode)
            {
                createNode = false;

                var GO = NavManager.inst.CreateNode(gameObject);
                Selection.activeGameObject = GO;
                createConnection = GO;
            }
            if (createGroupNode)
            {
                createGroupNode = false;

                var GO = NavManager.inst.CreateGroupNode(gameObject);
                Selection.activeGameObject = GO;
                createConnection = GO;
            }
            if (createConnection != null)
            {
                var otherNode = createConnection.GetComponent<NavNode>();
                if (otherNode != null)
                {
                    var navEdge = NavManager.inst.CreateEdge(this, otherNode);

                    neighbors.Add(otherNode, navEdge);
                    otherNode.neighbors.Add(this, navEdge);
                }

                createConnection = null;
            }

            #region Visualization

            //for (int i = 0; i < neighbors.Count; i++)
            //{
            //    var n = neighbors[i];
            //    if (n == null)
            //    {
            //        neighbors.RemoveAt(i);
            //        i--;
            //    }
            //    else
            //    {
            //        Debug.DrawLine(transform.position + Vector3.up * NavManager.NODE_RADIUS, n.transform.position + Vector3.up * NavManager.NODE_RADIUS, Color.green);
            //    }
            //}

            #endregion

            #endregion
        }
        if (Application.isPlaying || !NavManager.VISUALIZE)
        {
            render.enabled = false;
        }
    }
    
    public void OnDestroy()
    {
        foreach (var n in neighbors)
        {
            n.Key.GetNeighbors().Remove(this);
            DestroyImmediate(n.Value.gameObject);
        }

        NavManager.allNodes.Remove(this);
    }

    #region Public Functions

    public Dictionary<NavNode, NavEdge> GetNeighbors()
    {
        return neighbors;
    }

    #endregion
}
