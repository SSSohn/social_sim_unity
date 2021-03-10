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
    
    public override void StartNavigation(NavNode destination)
    {
        agent.ComputePath(destination.transform.position);
    }

    public override void StopNavigation(NavNode destination)
    {
        agent.StopAnimator();
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
