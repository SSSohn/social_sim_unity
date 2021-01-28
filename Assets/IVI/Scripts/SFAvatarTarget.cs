using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SFAvatarTarget : MonoBehaviour
{
    public float updateRate = 0.5f;
    public float maxLen = 20;

    private Vector3 destination;
    private SFAvatar agent;

    void Start()
    {
        agent = GetComponent<SFAvatar>();

        InitDest();
        StartCoroutine(Run());
    }

    public void InitDest()
    {
        var pos = new Vector3((Random.value - 0.5f) * 20, 0, (Random.value - 0.5f) * 20);
        NavMeshHit navMeshHit;
        NavMesh.SamplePosition(pos, out navMeshHit, float.MaxValue, NavMesh.AllAreas);
        destination = navMeshHit.position;
    }

    IEnumerator Run()
    {
        yield return null;

        while (true)
        {
            agent.ComputePath(destination);

            yield return new WaitForSeconds(updateRate);
        }

        yield break;
    }
}
