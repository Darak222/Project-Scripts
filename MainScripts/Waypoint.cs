using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public List<Waypoint> destinationList;
    public List<string> playerTags;
    public Dictionary<Waypoint, string> waypoints; 

    void Start(){
        waypoints = new Dictionary<Waypoint, string>();
        for(int i = 0; i < destinationList.Count; i++){
            waypoints.Add(destinationList[i], playerTags[i]);
        }
    }

    private void OnTriggerEnter(Collider other){
        Movement targetMovement = other.transform.GetComponent<Movement>();
        if(targetMovement != null){
            targetMovement.SetNextWaypoint(this);
        }
    }

}
