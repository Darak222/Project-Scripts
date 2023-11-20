using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawning : MonoBehaviour
{
    public Transform spawningPoint;
    public Transform spawnBox; 
    public Color unitColor;
    public Text uiSpawnTimer;

    public UnitsPerLevel unitsOnCurrentLevel;
    public UnitsInRace unitsInRace;


    [HideInInspector] public float timer = 0;
    public float spawnInterval;
    public Dictionary<Attacking.AttackType, int> attackUpgrades = new Dictionary<Attacking.AttackType, int>();
    public int defUpgrade = 0;
    public int hpUpgrade = 0;
    public enum InhibitorType{
        Middle,
        Left,
        Right
    }
    public InhibitorType inhibitorType;
    public int nexusLevel;
    private Material unitMaterial;
    private Building nexus;
    private bool isDestroyed = false;

    private List<Targetable> unitList;
    private bool isNexusAlive = true;
    

    void Start()
    {
        foreach(string attackTypeName in Enum.GetNames(typeof(Attacking.AttackType))){
            Attacking.AttackType attackType;
            if(Enum.TryParse(attackTypeName, out attackType)){
                if(!attackUpgrades.ContainsKey(attackType)){
                    attackUpgrades.Add(attackType, 0);
                }
            }
        }
        unitList = new List<Targetable>();
        this.transform.parent.GetComponent<Building>().onDeath += ShortenSpawnTimer;
        List<Building> buildingsList = GameplayManager.instance.FindNexusByPlayerTag(this.tag);
        if(buildingsList != null && buildingsList.Count > 0){
            nexus = buildingsList[0];
        }
        else{
            nexus = null;
        }
        AssignEvents();
    }

    public int GetNexusLevel(){
        if(nexus != null){
            return nexus.buildingLevel;
        }
        else{
            return nexusLevel;
        }
    }

    void OnDisable(){
        DeassignEvents();
    }

    private void AssignEvents()
    {
        if(nexus != null){
            nexus.onDeath += OnNexusDeath;
            nexus.onAttackUpgrade += AttackUpgrade;
            nexus.onDefUpgrade += DefUpgrade;
            nexus.onDefHpUpgrade += HpUpgrade;
        }
    }

    private void DeassignEvents(Targetable target = null, Attacking attacker = null)
    {
        if(nexus != null){
            nexus.onDeath -= OnNexusDeath;
            nexus.onAttackUpgrade -= AttackUpgrade;
            nexus.onDefUpgrade -= DefUpgrade;
            nexus.onDefHpUpgrade -= HpUpgrade;
        }
    }

    void Update()
    {
        SpawnUnitsByInterval();
    }

    private void SpawnUnitsByInterval()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            if (unitsInRace != null)
            {
                if (unitsOnCurrentLevel != null)
                {
                    for (int i = 0; i < unitsOnCurrentLevel.meleeUnits; i++)
                    {
                        SpawnUnit(unitsInRace.meleeUnit, false, "");
                    }
                    for (int i = 0; i < unitsOnCurrentLevel.rangedUnits; i++)
                    {
                        SpawnUnit(unitsInRace.rangedUnit, false, "");
                    }
                    for (int i = 0; i < unitsOnCurrentLevel.mageUnits; i++)
                    {
                        SpawnUnit(unitsInRace.mageUnit, false, "");
                    }
                    for (int i = 0; i < unitsOnCurrentLevel.riderUnits; i++)
                    {
                        SpawnUnit(unitsInRace.riderUnit, false, "");
                    }
                    for (int i = 0; i < unitsOnCurrentLevel.siegeUnits; i++){
                        SpawnUnit(unitsInRace.siegeUnit, false, "");
                    }
                }
            }
        }
        uiSpawnTimer.text = Mathf.FloorToInt(timer).ToString();
    }

    public void SpawnUnit(SpawnableUnit spawnUnit, bool isHero, string uniqueName){
        if(spawnUnit != null){
            SpawnableUnit newUnit = Instantiate(spawnUnit, spawningPoint.position, spawningPoint.rotation, spawnBox);
            newUnit.gameObject.tag = this.gameObject.tag;
            Targetable newUnitTargetable = newUnit.GetComponent<Targetable>();
            Attacking newUnitAttacking = newUnitTargetable.GetComponentInChildren<Attacking>();
            if(newUnitAttacking != null && attackUpgrades.ContainsKey(newUnitAttacking.attackType)){
                newUnitTargetable.IncreaseAttack(attackUpgrades[newUnitAttacking.attackType]);
            }
            newUnitTargetable.IncreaseDef(defUpgrade);
            newUnitTargetable.IncreaseMaxHp(hpUpgrade);
            newUnitTargetable.onDeath += RemoveUnitFromList;
            if(unitMaterial == null){
                unitMaterial = newUnit.GetComponent<MeshRenderer>().material;
                unitMaterial.color = unitColor;
            }
            newUnit.GetComponent<MeshRenderer>().material = unitMaterial;
            unitList.Add(newUnitTargetable);
            if(isHero){
                GameplayManager.instance.AddHero(this.gameObject.tag, uniqueName, newUnitTargetable);
            }
        }
    }

    void RemoveUnitFromList(Targetable target, Attacking attacker){
        if(unitList.Contains(target)){
            unitList.Remove(target);
        }
    }

    public void ShortenSpawnTimer(Targetable target, Attacking attack){
        if(isNexusAlive){
            spawnInterval *= 3;
            isDestroyed = true;
        }
        else{
            Destroy(this.gameObject);
        }
    }

    public void OnNexusDeath(Targetable target, Attacking attacker){
        if(attacker != null){
            nexusLevel = ((Building)target).buildingLevel;
            isNexusAlive = false;
            if(isDestroyed){
                Destroy(this.gameObject);
            }
        }
        else{
            List<Building> nexusList = GameplayManager.instance.FindNexusByPlayerTag(this.gameObject.tag);
            if(nexusList != null){
                foreach(Building building in nexusList){
                    if(building != nexus){
                        nexus = building;
                        AssignEvents();
                    }
                }
            }
        }
    }

    private void AttackUpgrade(int value, List<Attacking.AttackType> damageTypes){
        foreach(Attacking.AttackType damageType in damageTypes){
            if(attackUpgrades.ContainsKey(damageType)){
                attackUpgrades[damageType] += value;
            }
        }
    }

    private void DefUpgrade(int value){
        defUpgrade += value;
    }

    private void HpUpgrade(int value){
        hpUpgrade += value;
    }


}

