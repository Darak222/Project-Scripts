using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesUI : MonoBehaviour
{
    public GeneralSlot emptySlot;
    public Transform slotsRoot;
    private Targetable selectedTarget;
    private GeneralSlot[] slots;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ChangeTarget(Targetable newTarget){
        selectedTarget = newTarget;
        if(selectedTarget == null || GameplayManager.instance.currentPlayerTag != newTarget.gameObject.tag){
            if(slots != null && slots.Length > 0){
                for(int i = 0; i < slots.Length; i++){
                    if(slots[i] != null){
                        Destroy(slots[i].gameObject);
                    }
                }
            }
        }
        else{
            if(slots == null){
                slots = new GeneralSlot[selectedTarget.slots.Length];
            }
            if(slots.Length != 0){
                for(int i = 0; i < slots.Length; i++){
                    if(slots[i] != null){
                        Destroy(slots[i].gameObject);
                    }
                }
            }
            for(int i = 0; i < selectedTarget.slots.Length; i++){
                if(selectedTarget.slots[i] != null){
                    slots[i] = Instantiate(selectedTarget.slots[i], slotsRoot);
                }
                else{
                    slots[i] = Instantiate(emptySlot, slotsRoot);
                }
                slots[i].AssignTarget(selectedTarget);
            }
        }
    }

    public void TargetDestroyed(Targetable destroyedTarget){
        if(destroyedTarget == this.selectedTarget){
            ChangeTarget(null);
        }
    }

}
