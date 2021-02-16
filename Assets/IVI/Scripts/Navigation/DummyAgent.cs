using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DummyAgent : INavigable
{
    public override void StartNavigation(NavNode dest)
    {
        var nma = GetComponent<NavMeshAgent>();

        nma.isStopped = false;
        nma.SetDestination(dest.transform.position);
    }

    public override void StopNavigation(NavNode dest)
    {
        GetComponent<NavMeshAgent>().isStopped = true;
    }

    public override void StartGroup(GroupNavNode group)
    {
        group.members.Add(this);
    }

    public override void StopGroup(GroupNavNode group)
    {
        group.members.Remove(this);
    }

}
