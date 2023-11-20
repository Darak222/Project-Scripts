using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    public Text attack;
    public Text def;
    public Text hitPoints;
    public Text attackType;
    public Text defType;
    public Image avatar;
    public Targetable selectedTarget;
    Attacking selectedTargetAttacking;
    void Start()
    {
        
    }

    void Update()
    {
        UpdateStats();
    }

    private void UpdateStats()
    {
        if (selectedTarget != null)
        {
            hitPoints.text = string.Format("{0:#}/{1}", selectedTarget.GetCurrentHitPoints(), selectedTarget.GetMaxHp());
            def.text = selectedTarget.armor.ToString();
            if(selectedTargetAttacking != null){
                attackType.text = selectedTargetAttacking.attackType.ToString();
                attack.text = selectedTargetAttacking.damage.ToString();
            }
            defType.text = selectedTarget.defenceType.ToString();
        }
    }

    public void ChangeTarget(Targetable newTarget){
        selectedTarget = newTarget;
        if(selectedTarget == null){
            gameObject.SetActive(false);
        }
        else{
            def.text = selectedTarget.armor.ToString();
            hitPoints.text = string.Format("{0}/{1}", selectedTarget.GetCurrentHitPoints(), selectedTarget.GetMaxHp());
            avatar.enabled = true;
            avatar.sprite = selectedTarget.avatar;
            selectedTargetAttacking = selectedTarget.GetComponentInChildren<Attacking>();
            if(selectedTargetAttacking != null){
                attack.text = selectedTargetAttacking.damage.ToString();
            }
        }

    }

    public void TargetDestroyed(Targetable destroyedTarget){
        if(destroyedTarget == this.selectedTarget){
            ChangeTarget(null);
        }
    }
}
