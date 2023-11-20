using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeQueueUI : MonoBehaviour
{
    public Targetable selectedTarget;
    public Slider timer;
    public Image[] upgradeQueue;

    void Update(){
        if(selectedTarget != null){
            timer.value = selectedTarget.upgradeTimer;
            timer.maxValue = selectedTarget.GetUpgradeTime();
            for(int i = 0; i < upgradeQueue.Length; i++){
                if(i < selectedTarget.upgradeQueue.Count){
                    upgradeQueue[i].gameObject.SetActive(true);
                    upgradeQueue[i].sprite = selectedTarget.slotsData[selectedTarget.upgradeQueue[i]].image;
                }
                else{
                    upgradeQueue[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ChangeTarget(Targetable newTarget){
        selectedTarget = newTarget;
        if(selectedTarget == null){
            timer.gameObject.SetActive(false);
            foreach(Image upgrade in upgradeQueue){
                upgrade.gameObject.SetActive(false);
            }
        }
        else{
            timer.value = selectedTarget.upgradeTimer;
            timer.maxValue = selectedTarget.GetUpgradeTime();
            for(int i = 0; i < selectedTarget.upgradeQueue.Count; i++){
                upgradeQueue[i].gameObject.SetActive(true);
                upgradeQueue[i].sprite = selectedTarget.slotsData[selectedTarget.upgradeQueue[i]].image;
            }
            timer.gameObject.SetActive(true);
        }

    }

    public void TargetDestroyed(Targetable destroyedTarget){
        if(destroyedTarget == this.selectedTarget){
            ChangeTarget(null);
        }
    }
}
