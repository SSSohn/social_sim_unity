using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class INavigable : MonoBehaviour
{
    [HideInInspector]
    public NavNode destination;
    [HideInInspector]
    public Vector3 destinationPos;
    [HideInInspector]
    public bool navigating = false;

    public void Start()
    {
        StartCoroutine(Coroutine());
    }

    #region Abstract Behaviors

    public abstract void StartNavigation(NavNode destination, Vector3 offset);

    public abstract void StopNavigation();

    public abstract void StartGroup(GroupNavNode group);

    public abstract void StopGroup(GroupNavNode group);

    #endregion

    private IEnumerator Coroutine()
    {
        while (true)
        {
            if (CloseEnough())
            {
                StopNavigation();

                navigating = false;

                if (AtGroupNode())
                {
                    var groupNode = (GroupNavNode)destination;
                    var time = groupNode.GetTime();

                    transform.LookAt(groupNode.transform.position, Vector3.up);
                    //StartGroup(groupNode);
                    yield return new WaitForSeconds(time);
                    StopGroup(groupNode);
                }

                yield return NavManager.inst.UpdateAgentGoal(this);
            }

            yield return null;
        }
    }

    public void InitDest(NavNode destNode, Vector3 offset)
    {
        destination = destNode;

        navigating = true;

        StartNavigation(destination, offset);
    }

    private bool AtGroupNode()
    {
        return NavManager.inst.groupNode2Index.ContainsKey(destination);
    }

    private bool CloseEnough()
    {
        if (destination != null)
        {
            var destPos = destinationPos;
            var currPos = transform.position;
            currPos.y = 0;
            destPos.y = 0;
            return navigating && Vector3.Distance(destPos, currPos) <= (AtGroupNode() ? 0.5f : destination.radius);
        }

        return false;
    }
}
