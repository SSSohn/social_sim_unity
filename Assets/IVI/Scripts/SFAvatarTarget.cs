using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SFAvatarTarget : INavigable
{
    private SFAvatar agent;

    new void Start()
    {
        base.Start();

        agent = GetComponent<SFAvatar>();
    }
    
    public override void StartNavigation(NavNode destination, Vector3 offset)
    {
        destinationPos = destination.transform.position + offset;
        agent.ComputePath(destinationPos);
    }

    public override void StopNavigation()
    {
        agent.StopAnimator();
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
