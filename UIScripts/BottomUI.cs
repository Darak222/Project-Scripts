using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomUI : MonoBehaviour
{
    public StatsUI statsUi;
    public UpgradeQueueUI upgradeQueueUi;
    public UpgradesUI upgradesUi;

    void Start(){
        ToggleStats(true);
    }

    public void ToggleStats(bool isVisible){
        statsUi.gameObject.SetActive(isVisible);
        upgradeQueueUi.gameObject.SetActive(!isVisible);
    }
}
