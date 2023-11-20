using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class Bot : MonoBehaviour
{
    public string playerTag;
    Building nexus;
    Dictionary<Spawning.InhibitorType, Building> inhibitors;
    Targeting nexusTargeting;
    Dictionary<Spawning.InhibitorType, Targeting> inhibitorsTargeting;
    Dictionary<Spawning.InhibitorType, float> inhibitorHeroTimers;
    public enum ActionType{
        UpgradeNexus,
        UpgradeInhibitorMid,
        UpgradeInhibitorRight,
        UpgradeInhibitorLeft,
        CastSpell,
        UpgradeMeleeAttack,
        UpgradeRangedAttack,
        UpgradeMagicAttack,
        UpgradeBuildings,
        UpgradeDef,
        BuyMeleeUnitMid,
        BuyRangedUnitMid,
        BuyMageUnitMid,
        BuyRiderUnitMid,
        BuySiegeUnitMid,
        BuyHighKingMid,
        BuyHighPriestMid,
        BuyHeroSiegeMid,
        BuyArchmageMid,
        BuyMeleeUnitRight,
        BuyRangedUnitRight,
        BuyMageUnitRight,
        BuyRiderUnitRight,
        BuySiegeUnitRight,
        BuyHighKingRight,
        BuyHighPriestRight,
        BuyHeroSiegeRight,
        BuyArchmageRight,
        BuyMeleeUnitLeft,
        BuyRangedUnitLeft,
        BuyMageUnitLeft,
        BuyRiderUnitLeft,
        BuySiegeUnitLeft,
        BuyHighKingLeft,
        BuyHighPriestLeft,
        BuyHeroSiegeLeft,
        BuyArchmageLeft,
    }
    public List<ActionType> actionTypes;
    public List<string> actionUniqueNames;
    public delegate void BotAction(Targetable target);
    public List<ActionType> actions;
    public int enemiesToSpell;
    public int enemiesToMinion;
    public int goldToHero;
    public List<ActionType> aiTriggeredActions;
    public List<ActionType> midInhiHeroes;
    public List<ActionType> leftInhiHeroes;
    public List<ActionType> rightInhiHeroes;
    public int timeToHero;
    private Dictionary<ActionType, string> actionNamesByType;
    private int currentActionIndex = 0;
    GeneralSlot nextAction;
    Targetable nextActionTarget;
    ActionType nextActionType;
    

    void Awake(){
        actionNamesByType = new Dictionary<ActionType, string>();
        for(int i = 0; i < actionTypes.Count; i++){
            if(actionUniqueNames.Count > i){
                actionNamesByType.Add(actionTypes[i], actionUniqueNames[i]);
            }
        }
    }
    void Start(){
        inhibitorHeroTimers = new Dictionary<Spawning.InhibitorType, float>();
        inhibitors = new Dictionary<Spawning.InhibitorType, Building>();
        inhibitorsTargeting = new Dictionary<Spawning.InhibitorType, Targeting>();
        List<Building> buildings = GameplayManager.instance.GetBuildingsByPlayerTag(playerTag);
        foreach(Building building in buildings){
            if(building.buildingType == Building.BuildingTypes.Nexus){
                nexus = building;
                nexusTargeting = nexus.GetComponentInChildren<Targeting>();
                nexus.onDeath += OnBuildingDestroyed;
            }
            if(building.buildingType == Building.BuildingTypes.Inhibitor){
                Spawning spawning = building.GetComponentInChildren<Spawning>();
                if(!inhibitors.ContainsKey(spawning.inhibitorType)){
                    inhibitors.Add(spawning.inhibitorType, building);
                    inhibitorsTargeting.Add(spawning.inhibitorType, building.GetComponentInChildren<Targeting>());
                    inhibitorHeroTimers.Add(spawning.inhibitorType, 0);
                    building.onDeath += OnBuildingDestroyed;
                }
            }
        }
        GetActionByIndex(currentActionIndex);
    }

    void Update(){
        List<Spawning.InhibitorType> inhibitorHeroTimersKeys = new List<Spawning.InhibitorType>(inhibitorHeroTimers.Keys);
        foreach(Spawning.InhibitorType inhibitor in inhibitorHeroTimersKeys){
            inhibitorHeroTimers[inhibitor] = inhibitorHeroTimers[inhibitor] + Time.deltaTime;
        }
        if(nexusTargeting.enemiesInRange.Count >= enemiesToSpell){
            if(nexus.slotsData[actionNamesByType[ActionType.CastSpell]].CanBuy(GameplayManager.instance.GetPlayerGoldByTag(playerTag))){
                GetActionByType(ActionType.CastSpell);
            }
        }
        if(nextActionType != ActionType.CastSpell)
        {
            bool canHero = false;
            if(GameplayManager.instance.GetPlayerGoldByTag(playerTag) >= goldToHero){
                canHero = TryBuyHero(inhibitorsTargeting.Keys.ToList());
            }
            else if(!canHero){
                TryBuyUnit(inhibitorsTargeting.Keys.ToList());
            }
        }

        if(nextAction != null && nextActionTarget != null){
            if(nextActionTarget.slotsData[nextAction.uniqueName].CanBuy(GameplayManager.instance.GetPlayerGoldByTag(playerTag))){
                if(nextActionTarget.upgradeQueue == null || nextActionTarget.upgradeQueue.Count == 0){
                    nextAction.SlotAction(nextActionTarget, nextActionTarget.slotsData[nextAction.uniqueName]);
                    if(midInhiHeroes.Contains(nextActionType)){
                        inhibitorHeroTimers[Spawning.InhibitorType.Middle] = 0;
                    }
                    else if(leftInhiHeroes.Contains(nextActionType)){
                        inhibitorHeroTimers[Spawning.InhibitorType.Left] = 0;
                    }
                    else if(rightInhiHeroes.Contains(nextActionType)){
                        inhibitorHeroTimers[Spawning.InhibitorType.Right] = 0;
                    }
                    if(aiTriggeredActions.Contains(nextActionType)){
                        GetActionByIndex(currentActionIndex);
                    }
                    else{
                        GetActionByIndex(currentActionIndex + 1);
                    }
                }
            }
        }
        else{
            GetActionByIndex(currentActionIndex + 1);
        }
    }

    private void TryBuyUnit(List<Spawning.InhibitorType> inhibitorsCheck)
    {
        int maxEnemiesInRange = 0;
        Spawning.InhibitorType inhibitorWithMostEnemies = Spawning.InhibitorType.Middle;
        List<Spawning.InhibitorType> availableInhibitors = new List<Spawning.InhibitorType>();
        foreach(Spawning.InhibitorType inhibitor in inhibitorsCheck){
            if(inhibitors[inhibitor] != null){
                availableInhibitors.Add(inhibitor);
            }
        }
        inhibitorsCheck = availableInhibitors;
        if(inhibitorsCheck.Count == 0){
            return;
        }
        foreach (Spawning.InhibitorType inhibitor in inhibitorsCheck)
        {
            if (inhibitorsTargeting[inhibitor].enemiesInRange.Count >= enemiesToMinion)
            {
                if (inhibitorsTargeting[inhibitor].enemiesInRange.Count > maxEnemiesInRange)
                {
                    inhibitorWithMostEnemies = inhibitor;
                    maxEnemiesInRange = inhibitorsTargeting[inhibitor].enemiesInRange.Count;
                }
            }
        }
        if (maxEnemiesInRange > 0)
        {
            ActionType buyMelee = ActionType.BuyMeleeUnitMid;
            ActionType buyRanged = ActionType.BuyRangedUnitMid;
            ActionType buyMage = ActionType.BuyMageUnitMid;
            ActionType buyRider = ActionType.BuyRiderUnitMid;
            ActionType buySiege = ActionType.BuySiegeUnitMid;
            switch(inhibitorWithMostEnemies){
                case Spawning.InhibitorType.Left:
                    buyMelee = ActionType.BuyMeleeUnitLeft;
                    buyRanged = ActionType.BuyRangedUnitLeft;
                    buyMage = ActionType.BuyMageUnitLeft;
                    buyRider = ActionType.BuyRiderUnitLeft;
                    buySiege = ActionType.BuySiegeUnitLeft;
                    break;
                case Spawning.InhibitorType.Right:
                    buyMelee = ActionType.BuyMeleeUnitRight;
                    buyRanged = ActionType.BuyRangedUnitRight;
                    buyMage = ActionType.BuyMageUnitRight;
                    buyRider = ActionType.BuyRiderUnitRight;
                    buySiege = ActionType.BuySiegeUnitRight;
                    break;
                default:
                    break;
            }
            if (inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyMelee]].CanBuy(GameplayManager.instance.GetPlayerGoldByTag(playerTag)))
            {
                GetActionByType(buyMelee);
            }
            else if (inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyRanged]].CanBuy(GameplayManager.instance.GetPlayerGoldByTag(playerTag)))
            {
                GetActionByType(buyRanged);
            }
            else if (inhibitors[inhibitorWithMostEnemies].slotsData.ContainsKey(actionNamesByType[buyMage]) && inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyMage]].CanBuy(GameplayManager.instance.GetPlayerGoldByTag(playerTag)))
            {
                GetActionByType(buyMage);
            }
            else if (inhibitors[inhibitorWithMostEnemies].slotsData.ContainsKey(actionNamesByType[buyRider]) && inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyRider]].CanBuy(GameplayManager.instance.GetPlayerGoldByTag(playerTag)))
            {
                GetActionByType(buyRider);
            }
            else if (inhibitors[inhibitorWithMostEnemies].slotsData.ContainsKey(actionNamesByType[buySiege]) && inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buySiege]].CanBuy(GameplayManager.instance.GetPlayerGoldByTag(playerTag))){
                GetActionByType(buySiege);
            }
            else {
                inhibitorsCheck.Remove(inhibitorWithMostEnemies);
                if(inhibitorsCheck.Count > 0){
                    TryBuyUnit(inhibitorsCheck);
                }
            }
        }
    }

    private bool TryBuyHero(List<Spawning.InhibitorType> inhibitorsCheck){
        int maxEnemiesInRange = 0;
        Spawning.InhibitorType inhibitorWithMostEnemies = Spawning.InhibitorType.Middle;
        List<Spawning.InhibitorType> availableInhibitors = new List<Spawning.InhibitorType>();
        foreach(Spawning.InhibitorType inhibitor in inhibitorsCheck){
            if(inhibitorHeroTimers[inhibitor] >= timeToHero && inhibitors[inhibitor] != null){
                availableInhibitors.Add(inhibitor);

            }
        }
        inhibitorsCheck = availableInhibitors;
        if(inhibitorsCheck.Count == 0){
            return false;
        }
        foreach (Spawning.InhibitorType inhibitor in inhibitorsCheck)
        {
            if (inhibitorsTargeting[inhibitor].enemiesInRange.Count >= enemiesToMinion)
            {
                if (inhibitorsTargeting[inhibitor].enemiesInRange.Count > maxEnemiesInRange)
                {
                    inhibitorWithMostEnemies = inhibitor;
                    maxEnemiesInRange = inhibitorsTargeting[inhibitor].enemiesInRange.Count;
                }
            }
        }
        if (maxEnemiesInRange > 0)

        {
            ActionType buyHighKing = ActionType.BuyHighKingMid;
            ActionType buyHighPriest = ActionType.BuyHighPriestMid;
            ActionType buyHeroSiege = ActionType.BuyHeroSiegeMid;
            ActionType buyArchmage = ActionType.BuyArchmageMid;
            switch(inhibitorWithMostEnemies){
                case Spawning.InhibitorType.Left:
                    buyHighKing = ActionType.BuyHighKingLeft;
                    buyHighPriest = ActionType.BuyHighPriestLeft;
                    buyHeroSiege = ActionType.BuyHeroSiegeLeft;
                    buyArchmage = ActionType.BuyArchmageLeft;
                    break;
                case Spawning.InhibitorType.Right:
                    buyHighKing = ActionType.BuyHighKingRight;
                    buyHighPriest = ActionType.BuyHighPriestRight;
                    buyHeroSiege = ActionType.BuyHeroSiegeRight;
                    buyArchmage = ActionType.BuyArchmageRight;
                    break;
                default:
                    break;
            }
            if(inhibitors[inhibitorWithMostEnemies].slotsData.ContainsKey(actionNamesByType[buyHighKing]) && inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyHighKing]].CanBuy(GameplayManager.instance.GetPlayerGoldByTag(playerTag)) && inhibitors[inhibitorWithMostEnemies].GetComponentInChildren<Spawning>().GetNexusLevel() >= ((UnitSlotData)inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyHighKing]]).requiredNexusLevel && !GameplayManager.instance.IsHeroOnMap(playerTag, actionNamesByType[buyHighKing]))
            {
                GetActionByType(buyHighKing);
                return true;
            }
            else if(inhibitors[inhibitorWithMostEnemies].slotsData.ContainsKey(actionNamesByType[buyHighPriest]) && inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyHighPriest]].CanBuy(GameplayManager.instance.GetPlayerGoldByTag(playerTag)) && inhibitors[inhibitorWithMostEnemies].GetComponentInChildren<Spawning>().GetNexusLevel() >= ((UnitSlotData)inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyHighPriest]]).requiredNexusLevel && !GameplayManager.instance.IsHeroOnMap(playerTag, actionNamesByType[buyHighPriest]))
            {
                GetActionByType(buyHighPriest);
                return true;
            }
            else if(inhibitors[inhibitorWithMostEnemies].slotsData.ContainsKey(actionNamesByType[buyHeroSiege]) && inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyHeroSiege]].CanBuy(GameplayManager.instance.GetPlayerGoldByTag(playerTag)) && inhibitors[inhibitorWithMostEnemies].GetComponentInChildren<Spawning>().GetNexusLevel() >= ((UnitSlotData)inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyHeroSiege]]).requiredNexusLevel && !GameplayManager.instance.IsHeroOnMap(playerTag, actionNamesByType[buyHeroSiege]))
            {
                GetActionByType(buyHeroSiege);
                return true;
            }
            else if(inhibitors[inhibitorWithMostEnemies].slotsData.ContainsKey(actionNamesByType[buyArchmage]) && inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyArchmage]].CanBuy(GameplayManager.instance.GetPlayerGoldByTag(playerTag)) && inhibitors[inhibitorWithMostEnemies].GetComponentInChildren<Spawning>().GetNexusLevel() >= ((UnitSlotData)inhibitors[inhibitorWithMostEnemies].slotsData[actionNamesByType[buyArchmage]]).requiredNexusLevel && !GameplayManager.instance.IsHeroOnMap(playerTag, actionNamesByType[buyArchmage]))
            {
                GetActionByType(buyArchmage);
                return true;
            }            
            else{
                inhibitorsCheck.Remove(inhibitorWithMostEnemies);
                if(inhibitorsCheck.Count > 0){
                    return TryBuyHero(inhibitorsCheck);
                }
                else{
                    return false;
                }
            }
        }
        else{
            return false;
        }
    }

    void GetActionByIndex(int index){
        if(index >= actions.Count){
            nextAction = null;
            return;
        }        
        currentActionIndex = index;
        ActionType action = actions[index];
        GetActionByType(action);
    }

    void GetActionByType(ActionType action){ 
        Targetable target = null;
        switch(action){
            case ActionType.UpgradeNexus:
                if(nexus != null){
                    target = nexus.GetComponent<Targetable>();
                }
                break;
            case ActionType.UpgradeInhibitorMid:
                if(inhibitors[Spawning.InhibitorType.Middle] != null){
                    target = inhibitors[Spawning.InhibitorType.Middle].GetComponent<Targetable>();
                }
                break;
            case ActionType.UpgradeInhibitorRight:
                if(inhibitors[Spawning.InhibitorType.Right] != null){
                    target = inhibitors[Spawning.InhibitorType.Right].GetComponent<Targetable>();
                }
                break;
            case ActionType.UpgradeInhibitorLeft:
                if(inhibitors[Spawning.InhibitorType.Left] != null){
                    target = inhibitors[Spawning.InhibitorType.Left].GetComponent<Targetable>();
                }
                break;
            case ActionType.UpgradeMeleeAttack:
                if(nexus != null){
                    target = nexus.GetComponent<Targetable>();
                }
                break;
            case ActionType.UpgradeRangedAttack:
                if(nexus != null){
                    target = nexus.GetComponent<Targetable>();
                }
                break;
            case ActionType.UpgradeMagicAttack:
                if(nexus != null){
                    target = nexus.GetComponent<Targetable>();
                }
                break;
            case ActionType.UpgradeDef:
                if(nexus != null){
                    target = nexus.GetComponent<Targetable>();
                }
                break;
            case ActionType.UpgradeBuildings:
                if(nexus != null){
                    target = nexus.GetComponent<Targetable>();
                }
                break;
            case ActionType.CastSpell:
                if(nexus != null){
                    target = nexus.GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyMeleeUnitMid:
                if(inhibitors[Spawning.InhibitorType.Middle] != null){
                    target = inhibitors[Spawning.InhibitorType.Middle].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyMeleeUnitLeft:
                if(inhibitors[Spawning.InhibitorType.Left] != null){
                    target = inhibitors[Spawning.InhibitorType.Left].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyMeleeUnitRight:
                if(inhibitors[Spawning.InhibitorType.Right] != null){
                    target = inhibitors[Spawning.InhibitorType.Right].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyRangedUnitMid:
                if(inhibitors[Spawning.InhibitorType.Middle] != null){
                    target = inhibitors[Spawning.InhibitorType.Middle].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyRangedUnitLeft:
                if(inhibitors[Spawning.InhibitorType.Left] != null){
                    target = inhibitors[Spawning.InhibitorType.Left].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyRangedUnitRight:
                if(inhibitors[Spawning.InhibitorType.Right] != null){
                    target = inhibitors[Spawning.InhibitorType.Right].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyMageUnitMid:
                if(inhibitors[Spawning.InhibitorType.Middle] != null){
                    target = inhibitors[Spawning.InhibitorType.Middle].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyMageUnitLeft:
                if(inhibitors[Spawning.InhibitorType.Left] != null){
                    target = inhibitors[Spawning.InhibitorType.Left].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyMageUnitRight:
                if(inhibitors[Spawning.InhibitorType.Right] != null){
                    target = inhibitors[Spawning.InhibitorType.Right].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyRiderUnitMid:
                if(inhibitors[Spawning.InhibitorType.Middle] != null){
                    target = inhibitors[Spawning.InhibitorType.Middle].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyRiderUnitLeft:
                if(inhibitors[Spawning.InhibitorType.Left] != null){
                    target = inhibitors[Spawning.InhibitorType.Left].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyRiderUnitRight:
                if(inhibitors[Spawning.InhibitorType.Right] != null){
                    target = inhibitors[Spawning.InhibitorType.Right].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuySiegeUnitMid:
                if(inhibitors[Spawning.InhibitorType.Middle] != null){
                    target = inhibitors[Spawning.InhibitorType.Middle].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuySiegeUnitLeft:
                if(inhibitors[Spawning.InhibitorType.Left] != null){
                    target = inhibitors[Spawning.InhibitorType.Left].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuySiegeUnitRight:
                if(inhibitors[Spawning.InhibitorType.Right] != null){
                    target = inhibitors[Spawning.InhibitorType.Right].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyHighKingMid:
                if(inhibitors[Spawning.InhibitorType.Middle] != null){
                    target = inhibitors[Spawning.InhibitorType.Middle].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyHighKingLeft:
                if(inhibitors[Spawning.InhibitorType.Left] != null){
                    target = inhibitors[Spawning.InhibitorType.Left].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyHighKingRight:
                if(inhibitors[Spawning.InhibitorType.Right] != null){
                    target = inhibitors[Spawning.InhibitorType.Right].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyHighPriestMid:
                if(inhibitors[Spawning.InhibitorType.Middle] != null){
                    target = inhibitors[Spawning.InhibitorType.Middle].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyHighPriestLeft:
                if(inhibitors[Spawning.InhibitorType.Left] != null){
                    target = inhibitors[Spawning.InhibitorType.Left].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyHighPriestRight:
                if(inhibitors[Spawning.InhibitorType.Right] != null){
                    target = inhibitors[Spawning.InhibitorType.Right].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyHeroSiegeMid:
                if(inhibitors[Spawning.InhibitorType.Middle] != null){
                    target = inhibitors[Spawning.InhibitorType.Middle].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyHeroSiegeLeft:
                if(inhibitors[Spawning.InhibitorType.Left] != null){
                    target = inhibitors[Spawning.InhibitorType.Left].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyHeroSiegeRight:
                if(inhibitors[Spawning.InhibitorType.Right] != null){
                    target = inhibitors[Spawning.InhibitorType.Right].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyArchmageMid:
                if(inhibitors[Spawning.InhibitorType.Middle] != null){
                    target = inhibitors[Spawning.InhibitorType.Middle].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyArchmageLeft:
                if(inhibitors[Spawning.InhibitorType.Left] != null){
                    target = inhibitors[Spawning.InhibitorType.Left].GetComponent<Targetable>();
                }
                break;
            case ActionType.BuyArchmageRight:
                if(inhibitors[Spawning.InhibitorType.Right] != null){
                    target = inhibitors[Spawning.InhibitorType.Right].GetComponent<Targetable>();
                }
                break;
            default:
                break;
        }
        if(target != null && target._slots.ContainsKey(actionNamesByType[action])){
            nextAction = target._slots[actionNamesByType[action]];
            nextActionType = action;
            nextActionTarget = target;
        }
        else{
            nextAction = null;
        }
    }

    public void OnBuildingDestroyed(Targetable target, Attacking attacker){
        target.onDeath -= OnBuildingDestroyed;
        if(attacker == null){
            List<Building> buildings = GameplayManager.instance.GetBuildingsByPlayerTag(playerTag);
            Building destroyedBuilding = target.GetComponent<Building>();
            for(int i = 0; i < buildings.Count; i++){
                if(buildings[i].buildingType == destroyedBuilding.buildingType){
                    switch(destroyedBuilding.buildingType){
                        case Building.BuildingTypes.Nexus:
                            nexus = buildings[i];
                            nexusTargeting = nexus.GetComponentInChildren<Targeting>();
                            nexus.onDeath += OnBuildingDestroyed;
                            break;
                        case Building.BuildingTypes.Inhibitor:
                            Spawning targetSpawning = target.GetComponentInChildren<Spawning>();
                            Spawning buildingSpawning = buildings[i].GetComponentInChildren<Spawning>();
                            if(targetSpawning.inhibitorType == buildingSpawning.inhibitorType && targetSpawning != buildingSpawning){
                                inhibitors[targetSpawning.inhibitorType] = buildings[i];
                                inhibitorsTargeting[targetSpawning.inhibitorType] = buildings[i].GetComponentInChildren<Targeting>();
                                buildings[i].onDeath += OnBuildingDestroyed;
                            }
                            break;
                        default:
                            break;
                    }
                    Targetable newTarget = buildings[i].GetComponent<Targetable>();
                    if(nextActionTarget == target){
                        nextActionTarget = newTarget;
                    }
                }
            }
        }
        else{
            Building destroyedBuilding = target.GetComponent<Building>();
            switch(destroyedBuilding.buildingType){
                case Building.BuildingTypes.Nexus:
                    nexus = null;
                    nexusTargeting = null;
                    break;
                case Building.BuildingTypes.Inhibitor:
                    Spawning targetSpawning = target.GetComponentInChildren<Spawning>();
                    if(targetSpawning == null){
                        targetSpawning = ((Building)target).nextLevelSpawning;
                    }
                    inhibitors[targetSpawning.inhibitorType] = null;
                    inhibitorsTargeting[targetSpawning.inhibitorType] = null;
                    break;
            }
        }
    }
}
