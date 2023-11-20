using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
    private Targetable target;
    private NavMeshAgent agent;
    public Attacking attacking;
    public float movementSpeed;
    private Waypoint nextWaypoint;
    private Waypoint previousWaypoint;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = movementSpeed;
        Targeting targeting = GetComponentInChildren<Targeting>();
        if(targeting != null){
            targeting.targetChanged += ChangeTarget;
        }
    }
    void Update()
    {
        MoveTowardsTarget();
    }

    void ChangeTarget(Targetable newTarget){
        target = newTarget;
    }

    void MoveTowardsTarget(){
        if(target != null){
            if(!attacking.IsInRange(target)){
                agent.SetDestination(target.transform.position);
                agent.isStopped = false;
            }
            else{
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }
        else if(nextWaypoint != null){
            agent.SetDestination(nextWaypoint.transform.position);
            agent.isStopped = false;
        }
        else{
            nextWaypoint = GameplayManager.instance.GetClosestWaypoint(this.transform.position);
        }
    }

    public void SetNextWaypoint(Waypoint currentWaypoint){
        string playerTag = gameObject.tag;
        Waypoint chosenWaypoint = null;
        for(int i = currentWaypoint.destinationList.Count - 1; i >= 0; i--){
            Waypoint candidate = currentWaypoint.destinationList[i];
            if(previousWaypoint != candidate && currentWaypoint.waypoints.ContainsKey(candidate)){
                string[] waypointPlayerTags = currentWaypoint.waypoints[candidate].Split(',');
                for(int j = 0; j < waypointPlayerTags.Length; j++){
                    if(GameplayManager.instance.alivePlayerTags.Contains(waypointPlayerTags[j])){
                        chosenWaypoint = candidate;
                    }
                }
            }
        }
        if(chosenWaypoint == null){
            bool waypointFound = false;
            for(int i = 0; i < currentWaypoint.destinationList.Count; i++){
                if(!waypointFound){
                    Waypoint candidate = currentWaypoint.destinationList[i];
                    if(previousWaypoint != candidate){
                        chosenWaypoint = candidate;
                        waypointFound = true;
                    }
                }
            }
        }
        previousWaypoint = currentWaypoint;
        nextWaypoint = chosenWaypoint;

    }
}
