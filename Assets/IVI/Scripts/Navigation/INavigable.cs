using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class INavigable : MonoBehaviour
{
    public NavNode destination;
    public bool navigating = false;

    public void Start()
    {
        StartCoroutine(Coroutine());
    }

    #region Abstract Behaviors

    public abstract void StartNavigation(NavNode destination);

    public abstract void StopNavigation(NavNode destination);

    public abstract void StartGroup(GroupNavNode group);

    public abstract void StopGroup(GroupNavNode group);

    #endregion

    private IEnumerator Coroutine()
    {
        while (true)
        {
            if (CloseEnough())
            {
                StopNavigation(destination);

                navigating = false;

                if (AtGroupNode())
                {
                    var groupNode = (GroupNavNode)destination;
                    var time = groupNode.GetTime();

                    StartGroup(groupNode);
                    yield return new WaitForSeconds(time);
                    StopGroup(groupNode);
                }

                yield return NavManager.inst.UpdateAgentGoal(this);
            }

            yield return null;
        }
    }

    public void InitDest(NavNode destNode)
    {
        destination = destNode;

        navigating = true;

        StartNavigation(destination);
    }

    private bool AtGroupNode()
    {
        return NavManager.inst.groupNode2Index.ContainsKey(destination);
    }

    private bool CloseEnough()
    {
        if (destination != null)
        {
            var destPos = destination.transform.position;
            var currPos = transform.position;
            currPos.y = 0;
            destPos.y = 0;
            return navigating && Vector3.Distance(destPos, currPos) <= destination.radius;
        }

        return false;
    }
}
