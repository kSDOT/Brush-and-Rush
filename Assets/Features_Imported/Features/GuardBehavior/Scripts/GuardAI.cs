using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class GuardAI : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Transform[] waypoints;
    int waypointIndex;
    Vector3 target;
    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        Destination();
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(transform.position,target)<1)
        {
            IterateIndex();
            Destination();
        }
    }

    void Destination()
    {
        target = waypoints[waypointIndex].position;
        navMeshAgent.SetDestination(target);
    }

    void IterateIndex()
    {
        waypointIndex++;
        if(waypointIndex == waypoints.Length)
        {
            waypointIndex = 0;
        }

    }
}
