using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnableUnit : MonoBehaviour
{
    public Transform feet;
    void Awake(){
        this.transform.position = new Vector3(transform.position.x, transform.position.y - feet.position.y, transform.position.z);
    }
    
}
