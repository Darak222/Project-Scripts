using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting : MonoBehaviour
{
    private Targetable target;
    public delegate void TargetChanged(Targetable target);
    public TargetChanged targetChanged;
    public SphereCollider aggroTrigger;
    public float aggroRange;
    public List<Targetable> enemiesInRange;
    

    void Start()
    {
        if(aggroTrigger != null){
            aggroTrigger.radius = aggroRange;
        }
        enemiesInRange = new List<Targetable>();
    }

    void Update()
    {
        CheckTarget();
    }

    void CheckTarget()
    {
        if(target == null && enemiesInRange.Count > 0){
            Targetable newClosestTarget = GetClosestTargetInRange();
            ChangeTarget(newClosestTarget);
        }
    }

    private Targetable GetClosestTargetInRange(){
        Targetable newTarget = null;
        float closestDistance = float.MaxValue;
        List<Targetable> enemiesLeft = new List<Targetable>();
        if(enemiesInRange != null && enemiesInRange.Count > 0){
            foreach(Targetable enemy in enemiesInRange){
                if(enemy == null){
                    continue;
                }
                enemiesLeft.Add(enemy);
                float enemyDistance = Vector3.Distance(enemy.transform.position, this.transform.position);
                if(enemyDistance < closestDistance){
                    newTarget = enemy;
                    closestDistance = enemyDistance;
                }
            }
        }
        enemiesInRange = enemiesLeft;
        return newTarget;
    }

    private void OnTriggerEnter(Collider other){
        if(!other.isTrigger){
            if(GameplayManager.instance.playerTags.Contains(other.gameObject.tag)){
                if(!other.gameObject.CompareTag(transform.parent.gameObject.tag)){
                    Targetable newTarget = other.GetComponent<Targetable>();
                    if(newTarget != null){
                        enemiesInRange.Add(newTarget);
                        newTarget.onDeath += RemoveTarget;
                        if(enemiesInRange.Count == 1){
                            ChangeTarget(newTarget);
                        }
                    }
                }
            } 
        }
    }

    private void OnTriggerExit(Collider other){
        if(!other.isTrigger){
            Targetable newTarget = other.GetComponent<Targetable>();
            if(newTarget != null){
                RemoveTarget(newTarget);
            }
        }
    }

    void RemoveTarget(Targetable targetToRemove, Attacking killer){
        RemoveTarget(targetToRemove);
    }

    void RemoveTarget(Targetable targetToRemove){
        if(enemiesInRange.Contains(targetToRemove)){
            enemiesInRange.Remove(targetToRemove);
        }
        if(target == targetToRemove){
            target = null;
        }
    }
    
    private void ChangeTarget(Targetable newTarget){
        target = newTarget;
        if(targetChanged != null){
            targetChanged(target);
        }
    }
}
