using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroupNavNode : NavNode
{
    public int groupSize = 5;
    public float minTime = 5;
    public float maxTime = 10;

    public Dictionary<INavigable, int> members;
    private Queue<int> indices;
    private List<Vector3> positions;

    new void Start()
    {
        base.Start();
        
        members = new Dictionary<INavigable, int>();
        indices = new Queue<int>();
        positions = new List<Vector3>();
        for (int i = 0; i < groupSize; i++)
        {
            indices.Enqueue(i);

            var angle = 360f * i / groupSize;
            var pos = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)) * radius;

            positions.Add(pos);
        }
    }

    new void Update()
    {
        base.Update();
        
        if (members != null)
        {
            foreach (var member in members)
            {
                //Debug.DrawLine(transform.position, member.Key.transform.position, Color.green);
                //Debug.DrawLine(positions[member.Value] + transform.position, member.Key.transform.position, Color.green);
            }
        }
    }

    public float GetTime()
    {
        return Random.value * (maxTime - minTime) + minTime;
    }

    public Vector3 AddMember(INavigable agent)
    {
        if (members.ContainsKey(agent))
            return positions[members[agent]];

        var ind = indices.Dequeue();
        members.Add(agent, ind);

        return positions[ind];
    }

    public void RemoveMember(INavigable agent)
    {
        var ind = members[agent];
        members.Remove(agent);
        indices.Enqueue(ind);
    }

}
