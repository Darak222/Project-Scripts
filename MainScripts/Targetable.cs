using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Targetable : MonoBehaviour
{

    public float armor;
    public delegate void OnDeath(Targetable target, Attacking killer);
    public float armorDamageReduction;
    public event OnDeath onDeath;
    public Slider hpBar;
    public float hpRegen;
    public float hpRegenInterval;
    public Sprite avatar;
    public int bountyGold;
    public GeneralSlot[] slots = new GeneralSlot[12];
    public Dictionary<string, GeneralSlot> _slots;
    public Dictionary<string, SlotData> slotsData;
    public List<string> upgradeQueue;
    public float upgradeTimer{get; private set;}
    public float maxHp;
    public List<string> unitButtons;
    public List<string> spellButtons;
    public enum DefenceType{
        Light = 0,
        Heavy,
        Armored
    }
    public DefenceType defenceType;
    private float hpRegenTimer;
    protected float hitPoints = -1;
    



    void Awake(){
        hpBar.gameObject.SetActive(false);
        hitPoints += maxHp + 1;
        upgradeQueue = new List<string>();
        unitButtons = new List<string>();
        spellButtons = new List<string>();
        InitializeSlotsData();
    }

    protected virtual void Update(){
        hpRegenTimer += Time.deltaTime;
        if(hpRegenTimer >= hpRegenInterval){
            hpRegenTimer = 0;
            if(hitPoints < maxHp){
                hitPoints += hpRegen;
                hpBar.value = hitPoints / maxHp;
                if(hitPoints >= maxHp){
                    hitPoints = maxHp;
                    hpBar.gameObject.SetActive(false);
                }
            }
        }
        if(upgradeQueue.Count > 0)
        {
            upgradeTimer += Time.deltaTime;
            if (upgradeTimer >= GetUpgradeTime())
            {
                upgradeTimer = 0;
                ExecuteSlotFunction(upgradeQueue[0]);
                upgradeQueue.RemoveAt(0);
            }
        }
        if (GameplayManager.instance.bottomUi.statsUi.selectedTarget == this){
            if(upgradeQueue.Count > 0 && gameObject.tag == GameplayManager.instance.currentPlayerTag){
                GameplayManager.instance.bottomUi.ToggleStats(false);
            }
            else{
                GameplayManager.instance.bottomUi.ToggleStats(true);
            }
        }
        foreach(string slotName in unitButtons){
            UnitSlotData unitData = (UnitSlotData)slotsData[slotName];
            if(unitData.availableUnits >= unitData.maxUnitCount){
                unitData.unitTimer = 0;
            }
            else{
                unitData.unitTimer += Time.deltaTime;
                if(unitData.unitTimer >= unitData.unitCooldown){
                    unitData.availableUnits++;
                    unitData.unitTimer = 0;
                }
            }
        }
        foreach(string slotName in spellButtons){
            SpellSlotData spellData = (SpellSlotData)slotsData[slotName];
            if(spellData.isAvailable){
                spellData.timer = 0;
            }
            else{
                spellData.timer += Time.deltaTime;
                if(spellData.timer >= spellData.cooldown){
                    spellData.isAvailable = true;
                    spellData.timer = 0;
                }
            }
        }
    }

    public float GetUpgradeTime()
    {
        if(upgradeQueue.Count > 0){
            return slotsData[upgradeQueue[0]].GetUpgradeTime();
        }
        else{
            return 1;
        }
    }

    public void InitializeSlotsData(){
        if(_slots == null){
            _slots = new Dictionary<string, GeneralSlot>();
        }
        if(slotsData == null){
            slotsData = new Dictionary<string, SlotData>();
            foreach(GeneralSlot slot in slots){
                if(slot != null && !string.IsNullOrEmpty(slot.uniqueName))
                {
                    InitializeSlotData(slot);
                }
            }
        }
        else{
            foreach(GeneralSlot slot in slots){
                if(slot != null && !string.IsNullOrEmpty(slot.uniqueName)){
                    if(slotsData.ContainsKey(slot.uniqueName)){
                        SlotData data = slotsData[slot.uniqueName];
                        switch (slot.dataType){
                            case GeneralSlot.SlotDataType.UpgradeBuilding:
                                ((UpgradeBuildingData)data).maxLevel = ((UpgradeBuildingSlot)slot).maxLevel;
                                break;
                            case GeneralSlot.SlotDataType.UpgradeUnit:
                                ((UpgradeSlotData)data).maxLevel = ((UpgradeUnitSlot)slot).maxLevel;
                                break;
                            case GeneralSlot.SlotDataType.Unit:
                                break;
                            case GeneralSlot.SlotDataType.Spell:
                                break;
                            case GeneralSlot.SlotDataType.LevelUpBuilding:
                                ((SlotData)data).price = ((UpgradeSlot)slot).price;
                                break;
                            default:
                                break;
                        }
                    }
                    else{
                        InitializeSlotData(slot);
                    }
                }
            }
        }
    }

    private void InitializeSlotData(GeneralSlot slot)
    {
        SlotData data;
        if(!_slots.ContainsKey(slot.uniqueName)){
            _slots.Add(slot.uniqueName, slot);
        }
        switch (slot.dataType)
        {
            case GeneralSlot.SlotDataType.UpgradeBuilding:
                data = new UpgradeBuildingData();
                ((UpgradeBuildingData)data).initialPrice = ((UpgradeBuildingSlot)slot).initialPrice;
                ((UpgradeBuildingData)data).priceIncrease = ((UpgradeBuildingSlot)slot).priceIncrease;
                ((UpgradeBuildingData)data).maxLevel = ((UpgradeBuildingSlot)slot).maxLevel;
                ((UpgradeBuildingData)data).upgradeTime = ((UpgradeBuildingSlot)slot).upgradeTime;
                ((UpgradeBuildingData)data).passiveGoldIncrease = ((UpgradeBuildingSlot)slot).passiveGoldIncrease;
                ((UpgradeBuildingData)data).upgradeTimeIncrease = ((UpgradeBuildingSlot)slot).upgradeTimeIncrease;
                ((UpgradeBuildingData)data).image = ((UpgradeBuildingSlot)slot).icon;
                break;
            case GeneralSlot.SlotDataType.UpgradeUnit:
                data = new UpgradeSlotData();
                ((UpgradeSlotData)data).initialPrice = ((UpgradeUnitSlot)slot).initialPrice;
                ((UpgradeSlotData)data).priceIncrease = ((UpgradeUnitSlot)slot).priceIncrease;
                ((UpgradeSlotData)data).maxLevel = ((UpgradeUnitSlot)slot).maxLevel;
                ((UpgradeSlotData)data).upgradeTime = ((UpgradeUnitSlot)slot).upgradeTime;
                ((UpgradeSlotData)data).upgradeType = ((UpgradeUnitSlot)slot).upgradeType;
                ((UpgradeSlotData)data).upgradeTimeIncrease = ((UpgradeUnitSlot)slot).upgradeTimeIncrease;
                ((UpgradeSlotData)data).image = ((UpgradeUnitSlot)slot).icon;
                break;
            case GeneralSlot.SlotDataType.Unit:
                data = new UnitSlotData();
                ((UnitSlotData)data).unitPrice = ((BuyUnitSlot)slot).unitPrice;
                ((UnitSlotData)data).unit = ((BuyUnitSlot)slot).unit;
                ((UnitSlotData)data).unitCooldown = ((BuyUnitSlot)slot).unitCooldown;
                ((UnitSlotData)data).availableUnits = ((BuyUnitSlot)slot).availableUnits;
                ((UnitSlotData)data).maxUnitCount = ((BuyUnitSlot)slot).maxUnitCount;
                ((UnitSlotData)data).image = ((BuyUnitSlot)slot).icon;
                ((UnitSlotData)data).requiredNexusLevel = ((BuyUnitSlot)slot).requiredNexusLevel;
                ((UnitSlotData)data).isHero = ((BuyUnitSlot)slot).isHero;
                unitButtons.Add(slot.uniqueName);
                break;
            case GeneralSlot.SlotDataType.Spell:
                data = new SpellSlotData();
                ((SpellSlotData)data).price = ((SpellSlot)slot).price;
                ((SpellSlotData)data).cooldown = ((SpellSlot)slot).cooldown;
                ((SpellSlotData)data).spell = ((SpellSlot)slot).spell;
                ((SpellSlotData)data).image = ((SpellSlot)slot).icon;
                spellButtons.Add(slot.uniqueName);
                break;
            case GeneralSlot.SlotDataType.LevelUpBuilding:
                data = new SlotData();
                ((SlotData)data).price = ((UpgradeSlot)slot).price;
                ((SlotData)data).upgradeTime = ((UpgradeSlot)slot).upgradeTime;
                ((SlotData)data).image = ((UpgradeSlot)slot).icon;
                break;
            default:
                data = null;
                break;
        }
        if (data != null)
        {
            data.uniqueName = slot.uniqueName;
            slotsData.Add(slot.uniqueName, data);
        }
    }

    public void TakeDamage(float damage, Attacking attacker, Attacking.AttackType damageType){
        if(hitPoints <= 0){
            return;
        }
        hitPoints -= CalcDamage(damage, damageType);
        if(hitPoints < 0){
            hitPoints = 0;
        }
        hpBar.gameObject.SetActive(true);
        hpBar.value = hitPoints / maxHp;
        if(hitPoints == 0)
        {
            Die(attacker);
        }
    }

    protected virtual void Die(Attacking attacker)
    {
        onDeath(this, attacker);
        GameplayManager.instance.bottomUi.statsUi.TargetDestroyed(this);
        GameplayManager.instance.bottomUi.upgradeQueueUi.TargetDestroyed(this);
        GameplayManager.instance.bottomUi.upgradesUi.TargetDestroyed(this);
        Destroy(this.gameObject);
    }

    private float CalcDamage(float damageToCalc, Attacking.AttackType damageType){
        float damageMultiplier = 0;
        switch(damageType){
            case Attacking.AttackType.Melee:
                switch(defenceType){
                    case DefenceType.Light:
                        damageMultiplier = 1;
                        break;
                    case DefenceType.Heavy:
                        damageMultiplier = 0.75f;
                        break;
                    case DefenceType.Armored:
                        damageMultiplier = 0.75f;
                        break;
                    default:
                        break;
                }
                break;
            case Attacking.AttackType.Ranged:
                switch(defenceType){
                    case DefenceType.Light:
                        damageMultiplier = 1.25f;
                        break;
                    case DefenceType.Heavy:
                        damageMultiplier = 0.75f;
                        break;
                    case DefenceType.Armored:
                        damageMultiplier = 0.5f;
                        break;
                    default:
                        break;
                }
                break;
            case Attacking.AttackType.Magic:
                switch(defenceType){
                    case DefenceType.Light:
                        damageMultiplier = 0.75f;
                        break;
                    case DefenceType.Heavy:
                        damageMultiplier = 1.5f;
                        break;
                    case DefenceType.Armored:
                        damageMultiplier = 0.25f;
                        break;
                    default:
                        break;
                }
                break;
            case Attacking.AttackType.Siege:
                switch(defenceType){
                    case DefenceType.Light:
                        damageMultiplier = 0.5f;
                        break;
                    case DefenceType.Heavy:
                        damageMultiplier = 0.5f;
                        break;
                    case DefenceType.Armored:
                        damageMultiplier = 1.5f;
                        break;
                    default:
                        break;
                }
                break;
            case Attacking.AttackType.DefStructure:
                damageMultiplier = 1;
                break;
            default:
                break;
        }
        float finalDamage = (damageToCalc - armorDamageReduction * damageToCalc * armor) * damageMultiplier;
        if(finalDamage <= 1){
            return 1 * damageMultiplier;
        }
        else{
            return finalDamage;
        }
    }

    public float GetMaxHp(){
        return this.maxHp;
    }

    public void SetHitPoints(float setHp){
        hitPoints = setHp;
    }

    public float GetCurrentHitPoints(){
        return hitPoints;
    }

    public void IncreaseMaxHp(int value){
        this.maxHp += value;
        this.hitPoints += value;
    }

    public void IncreaseDef(int value){ 
        this.armor += value;
    }

    public void IncreaseAttack(int value){
        this.GetComponentInChildren<Attacking>().damage += value;
    }

    public void ExecuteSlotFunction(string slotId){
        foreach(GeneralSlot slot in slots){
            if(slot != null && slot.uniqueName == slotId){
                switch(slot.dataType){
                    case GeneralSlot.SlotDataType.UpgradeUnit:
                        UpgradeUnitSlot.Upgrade(this, (UpgradeSlotData)slotsData[slotId]);
                        break;
                    case GeneralSlot.SlotDataType.Unit:
                        BuyUnitSlot.BuyUnit(this, (UnitSlotData)slotsData[slotId]);
                        break;
                    case GeneralSlot.SlotDataType.Spell:
                        SpellSlot.UseSpell(this, (SpellSlotData)slotsData[slotId]);
                        break;
                    case GeneralSlot.SlotDataType.UpgradeBuilding:
                        UpgradeBuildingSlot.Upgrade(this, (UpgradeBuildingData)slotsData[slotId]);
                        break;
                    case GeneralSlot.SlotDataType.LevelUpBuilding:
                        UpgradeSlot.Upgrade(this, slotsData[slotId]);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void AddSlotToQueue(string slotId){
        if(upgradeQueue.Count == 0){
            upgradeTimer = 0;
        }
        upgradeQueue.Add(slotId);
    }

    public bool IsSlotQueued(string slotId){
        return upgradeQueue.Contains(slotId);
    }
}

