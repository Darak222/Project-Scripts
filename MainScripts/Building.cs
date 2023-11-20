using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Targetable
{
    public Building nextLevel;
    public enum BuildingTypes{
        Nexus = 0,
        Inhibitor,
        Tower
    }

    public delegate void OnUpgrade(int value);
    public delegate void OnAttackUpgrade(int value, List<Attacking.AttackType> damageTypes);

    public event OnAttackUpgrade onAttackUpgrade;
    public event OnUpgrade onDefUpgrade;
    public event OnUpgrade onDefHpUpgrade;

    public BuildingTypes buildingType;

    public int attackUpgrade, defUpgrade, hpUpgrade;
    public float goldIncomeTimer;
    public int buildingLevel;
    public Spawning nextLevelSpawning;
    private float goldTimer;

    protected override void Update(){
        base.Update();
        if(buildingType == BuildingTypes.Nexus){
            goldTimer += Time.deltaTime;
            if(goldTimer >= goldIncomeTimer){
                goldTimer = 0;
                GameplayManager.instance.IncreasePlayerGold(gameObject.tag);
            }
        }
    }

    public void UpgradeBuilding(){
        Building building = Instantiate(nextLevel, transform.position, transform.rotation, transform.parent);
        building.SetHitPoints(hitPoints + building.maxHp - maxHp);
        building.gameObject.tag = this.gameObject.tag;
        building.RefreshStats(this);
        building.slotsData = this.slotsData;
        building.upgradeQueue = this.upgradeQueue;
        building.InitializeSlotsData();
        switch(this.buildingType){
            case BuildingTypes.Inhibitor:
                nextLevelSpawning = building.GetComponentInChildren<Spawning>();
                Spawning currentLevelSpawning = this.GetComponentInChildren<Spawning>();
                nextLevelSpawning.gameObject.tag = this.gameObject.tag;
                nextLevelSpawning.timer = currentLevelSpawning.timer;
                nextLevelSpawning.unitColor = currentLevelSpawning.unitColor;
                nextLevelSpawning.spawnBox = currentLevelSpawning.spawnBox;
                nextLevelSpawning.attackUpgrades = currentLevelSpawning.attackUpgrades;
                nextLevelSpawning.hpUpgrade = currentLevelSpawning.hpUpgrade;
                nextLevelSpawning.defUpgrade = currentLevelSpawning.defUpgrade;
                nextLevelSpawning.nexusLevel = currentLevelSpawning.nexusLevel;
                nextLevelSpawning.inhibitorType = currentLevelSpawning.inhibitorType;
                break;
            case BuildingTypes.Nexus:
                break;
            default:
                break;
        }
        building.buildingLevel = this.buildingLevel + 1;
        GameplayManager.instance.AddSingleBuilding(building);
        Die(null);
    }

    protected override void Die(Attacking attacker)
    {
        if(this.buildingType == BuildingTypes.Inhibitor && attacker != null){
            nextLevelSpawning = this.GetComponentInChildren<Spawning>();
            nextLevelSpawning.transform.parent = this.transform.parent;  
        }
        base.Die(attacker);
    }

    public void UpgradeUnitAttack(List<Attacking.AttackType> damageTypes){
        onAttackUpgrade(2, damageTypes);
    }

    public void UpgradeUnitDef(){
        onDefUpgrade(2);
        onDefHpUpgrade(25);
    }

    public void UpgradeBuildingStats(){
        List<Building> playerBuilding = GameplayManager.instance.GetBuildingsByPlayerTag(this.gameObject.tag);
        foreach(Building building in playerBuilding){
            building.IncreaseDef(2);
            building.IncreaseMaxHp(500);
            building.IncreaseAttack(50);
            building.attackUpgrade += 50;
            building.defUpgrade += 2;
            building.hpUpgrade += 500;
        }

    }

    public void RefreshStats(Building prevBuilding){
        attackUpgrade = prevBuilding.attackUpgrade;
        defUpgrade = prevBuilding.defUpgrade;
        hpUpgrade = prevBuilding.hpUpgrade;
        IncreaseDef(defUpgrade);
        IncreaseMaxHp(hpUpgrade);
        IncreaseAttack(attackUpgrade);
    }
}
