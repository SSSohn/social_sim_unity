using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NavEdge : MonoBehaviour
{
    public NavNode node1, node2;
    public float width;
    public Activity activity = Activity.DEFAULT;

    private MeshRenderer render;

    #region Enums

    public enum Activity
    {
        DEFAULT,
        NONE,
        NORMAL_FLOW
    }

    #endregion

    void Start()
    {
        render = GetComponent<MeshRenderer>();
        NavManager.allEdges.Add(this);

        width = NavManager.EDGE_WIDTH;
    }
    
    void Update()
    {
        var dir = (node2.transform.position - node1.transform.position).normalized;
        transform.position = (node1.transform.position + dir * node1.radius + node2.transform.position - dir * node2.radius) / 2;
        transform.LookAt(node1.transform, Vector3.up);
        transform.localScale = new Vector3(width, NavManager.EDGE_HEIGHT * 2, Vector3.Distance(node1.transform.position, node2.transform.position) - node1.radius - node2.radius);

        if (Application.isEditor)
        {
            #region Editor

            if (render == null)
                render = GetComponent<MeshRenderer>();

            render.enabled = true;

            NavManager.allEdges.Add(this);

            #endregion
        }
        if (Application.isPlaying || !NavManager.VISUALIZE)
        {
            render.enabled = false;
        }
    }

    private void OnDestroy()
    {
        NavManager.allEdges.Remove(this);
    }

}
