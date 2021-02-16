using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupNavNode : NavNode
{
    public int groupSize = 5;
    public float minTime = 5;
    public float maxTime = 10;

    public HashSet<INavigable> members;

    new void Start()
    {
        base.Start();

        members = new HashSet<INavigable>();
    }

    new void Update()
    {
        base.Update();

        if (members != null)
        {
            foreach (var member in members)
            {
                Debug.DrawLine(transform.position, member.transform.position, Color.green);
            }
        }
    }

    public float GetTime()
    {
        return Random.value * (maxTime - minTime) + minTime;
    }

}
