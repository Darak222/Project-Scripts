using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Attacking : MonoBehaviour
{
    public float attackRange;
    public float damage;
    public float attackInterval;
    public SphereCollider attackTrigger;
    public enum AttackType{
        Melee = 0,
        Ranged,
        Magic,
        Siege,
        DefStructure,
        Count
    }
    public AttackType attackType;
    private float timeSinceLastAttack;
    private Targetable target;
    public List<Targetable> enemiesInRange;

    void Start()
    {
        if(attackTrigger != null){
            attackTrigger.radius = attackRange;
        }
        Targeting targeting = transform.parent.GetComponentInChildren<Targeting>();
        if(targeting != null){
            targeting.targetChanged += ChangeTarget;
        }
    }

    void Update()
    {
        Attack();
    }

    private void Attack()
    {
        timeSinceLastAttack += Time.deltaTime;
        if (target != null && IsInRange(target))
        {
            if (timeSinceLastAttack >= attackInterval)
            {
                target.TakeDamage(damage, this, attackType);
                timeSinceLastAttack = 0;
            }
        }
    }

    void ChangeTarget(Targetable newTarget){
        target = newTarget;
    }

    private void OnTriggerEnter(Collider other){
        if(!other.isTrigger){
            if(GameplayManager.instance.playerTags.Contains(other.gameObject.tag)){
                if(!other.gameObject.CompareTag(transform.parent.gameObject.tag)){
                    Targetable newTarget = other.GetComponent<Targetable>();
                    if(newTarget != null){
                        enemiesInRange.Add(newTarget);
                        newTarget.onDeath += RemoveKilledTarget;
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

    void RemoveKilledTarget(Targetable targetToRemove, Attacking attacker){
        if(enemiesInRange.Contains(targetToRemove)){
            if(attacker != null && this == attacker ){
                GameplayManager.instance.AddBountyToPlayer(targetToRemove.bountyGold, attacker.transform.parent.tag);
            }
            enemiesInRange.Remove(targetToRemove);
            targetToRemove.onDeath -= RemoveKilledTarget;
        }
    }

    void RemoveTarget(Targetable targetToRemove){
        if(enemiesInRange.Contains(targetToRemove)){
            enemiesInRange.Remove(targetToRemove);
            targetToRemove.onDeath -= RemoveKilledTarget;
        }
    }

    public bool IsInRange(Targetable target){
        return enemiesInRange.Contains(target);
    }
    
}
