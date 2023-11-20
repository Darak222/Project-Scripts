using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpell : Spell
{
    public float startingRadius;
    public float expansionSpeed;
    public float expansionTime;
    public int damage;
    private float timer = 0;
    private new SphereCollider collider;
    private ParticleSystem.ShapeModule particleSystemShape;


    void Start(){
        collider = GetComponent<SphereCollider>();
        collider.radius = startingRadius;
        particleSystemShape = GetComponent<ParticleSystem>().shape;
        particleSystemShape.radius = startingRadius;
    }

    void Update(){
        if(timer < expansionTime){
            timer += Time.deltaTime;
            if(timer >= expansionTime){
                Destroy(this.gameObject, 2);
                ParticleSystem.EmissionModule particleSystemEmission = GetComponent<ParticleSystem>().emission;
                particleSystemEmission.rateOverTime = 0;
                collider.enabled = false;
            }
            collider.radius = startingRadius + timer * expansionSpeed;
            particleSystemShape.radius = startingRadius + timer * expansionSpeed;
        }
    }

    void OnTriggerEnter(Collider other){
        if(!other.isTrigger){
            if(GameplayManager.instance.playerTags.Contains(other.gameObject.tag)){
                if(!other.gameObject.CompareTag(transform.parent.parent.gameObject.tag)){
                    Targetable newTarget = other.GetComponent<Targetable>();
                    if(newTarget != null){
                        newTarget.TakeDamage(damage, attacker, Attacking.AttackType.Magic);
                    }
                }
            } 

        }
    }
}
