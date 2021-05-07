using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DummyAgent : INavigable
{

    new void Start()
    {
        base.Start();
    }

    public override void StartNavigation(NavNode dest, Vector3 offset)
    {
        var nma = GetComponent<NavMeshAgent>();

        nma.isStopped = false;
        nma.SetDestination(dest.transform.position);
    }

    public override void StopNavigation()
    {
        GetComponent<NavMeshAgent>().isStopped = true;
    }

    public override void StartGroup(GroupNavNode group)
    {
        group.AddMember(this);
    }

    public override void StopGroup(GroupNavNode group)
    {
        group.RemoveMember(this);
    }

}
