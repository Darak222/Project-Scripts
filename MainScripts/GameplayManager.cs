using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager instance;
    public List<string> playerTags;
    public List<Building> buildings;
    public Dictionary<string, PlayerDif> playerDiffs;
    public string currentPlayerTag; 
    public float timer = 0;
    public BottomUI bottomUi;
    public List<string> alivePlayerTags;
    public Dictionary<string, Bot> bots;
    public int towerValue, inhibitorValue, nexusValue;
    public Dictionary<string, string> tagToRace;
    public int startingBotGold;
    public int startingPlayerGold;
    public Dictionary<string, Dictionary<string, Targetable>> playerHeroesOnMap;
    private List<Waypoint> waypoints;

    void Awake(){
        if(instance == null){
            instance = this;
            waypoints = FindObjectsOfType<Waypoint>().ToList();
        }
        else{
            DestroyImmediate(this.gameObject);
        }
        AssignBots();
        if(MenuManager.instance != null){
            startingBotGold = MenuManager.instance.difficultyGoldStarter;
            startingPlayerGold = 500;
            CreatePlayerDiffs();
            SelectPlayer(MenuManager.instance.chosenPlayerTag);
        }
        else{
            CreatePlayerDiffs();
            SelectPlayer("Player 2");
        }
        playerDiffs[currentPlayerTag].gold = startingPlayerGold;
        AssignBuildings();
        alivePlayerTags = new List<string>();
        foreach(string playerTag in playerTags){
            alivePlayerTags.Add(playerTag);
        }
        CreateTagToRace();
    }

    void Update(){
         timer += Time.deltaTime;
    }

    public void SelectPlayer(string playerTag){
        if(bots.ContainsKey(playerTag)){
            bots[playerTag].gameObject.SetActive(false);
        }
        FindObjectOfType<CameraMovement>().SetStartingPosition(playerTag);
        currentPlayerTag = playerTag;
    }
    public void AssignBots(){
        bots = new Dictionary<string, Bot>();
        List<Bot> botsList = FindObjectsOfType<Bot>().ToList();
        foreach(Bot bot in botsList){
            if(!bots.ContainsKey(bot.playerTag)){
                bots.Add(bot.playerTag, bot);
            }
        }
    }


    public Waypoint GetClosestWaypoint(Vector3 position){
        Waypoint chosenOne = null;
        float closestDistance = float.MaxValue;
        float currentDistance;
        for(int i = 0; i < waypoints.Count; i++){
            currentDistance = Vector3.Distance(position, waypoints[i].transform.position);
            if(currentDistance < closestDistance){
                chosenOne = waypoints[i];
                closestDistance = currentDistance;
            }
        }
        return chosenOne;
    }

    void AssignBuildings(){
        buildings = FindObjectsOfType<Building>().ToList();
        for(int i = 0; i < buildings.Count; i++){
            buildings[i].onDeath += RemoveBuilding;
            if(buildings[i].buildingType == Building.BuildingTypes.Nexus && currentPlayerTag == buildings[i].gameObject.tag){
                FindObjectOfType<PlayerController>().ChangeTarget(buildings[i].GetComponent<Targetable>());
            }
        }
    }

    public void AddSingleBuilding(Building newBuilding){
        if(newBuilding != null){
            newBuilding.onDeath += RemoveBuilding;
            buildings.Add(newBuilding);
        }
    }

    void RemoveBuilding(Targetable removeBuilding, Attacking killer){
        if(buildings.Contains(removeBuilding)){
            string tag = removeBuilding.gameObject.tag;
            if(playerDiffs.ContainsKey(tag) && killer != null){
                switch(((Building)removeBuilding).buildingType){
                    case Building.BuildingTypes.Nexus:
                        playerDiffs[tag].gold += nexusValue;
                        break;
                    case Building.BuildingTypes.Inhibitor:
                        playerDiffs[tag].gold += inhibitorValue;
                        break;
                    case Building.BuildingTypes.Tower:
                        playerDiffs[tag].gold += towerValue;
                        break;
                }
            }
            BuildingDestroyed((Building)removeBuilding);
        }
    }

    void CreatePlayerDiffs(){
        playerDiffs = new Dictionary<string, PlayerDif>(); 
        foreach(string tag in playerTags){
            playerDiffs.Add(tag, new PlayerDif());
            playerDiffs[tag].gold = startingBotGold;
        }
        currentPlayerTag = playerTags[0];
    }

    public int GetCurrentPlayerGold(){
        return GetPlayerGoldByTag(currentPlayerTag);
    }

    public int GetPlayerGoldByTag(string playerTag){
        if(playerDiffs.ContainsKey(playerTag)){
            return playerDiffs[playerTag].gold;
        }
        else{
            return 0;
        }
    }

    public void CurrentPlayerGoldSpent(int cost){
        PlayerByTagGoldSpent(cost, currentPlayerTag);
    }

    public void PlayerByTagGoldSpent(int cost, string playerTag){
        if(playerDiffs.ContainsKey(playerTag)){
            playerDiffs[playerTag].gold -= cost;
        }
    }

    public void ChangeCurrentPlayer(string tag){
        if(playerTags.Contains(tag)){
            currentPlayerTag = tag;
        }
    }

    public void AddBountyToPlayer(int bounty, string tag){
        if(playerDiffs.ContainsKey(tag)){
            playerDiffs[tag].gold += bounty;
        }
    }

    public List<Building> FindNexusByPlayerTag(string playerTag){
        List<Building> nexusFound = new List<Building>();
        foreach(Building building in buildings){
            if(building.buildingType == Building.BuildingTypes.Nexus && building.gameObject.tag == playerTag){
                nexusFound.Add(building);
            }
        }
        return nexusFound;
    }

    public List<Building> GetBuildingsByPlayerTag(string playerTag){
        List<Building> playerBuildings = new List<Building>();
        foreach(Building building in buildings){
            if(building.gameObject.tag == playerTag){
                playerBuildings.Add(building);
            }
        }
        return playerBuildings;
    }

    public PlayerDif GetPlayerDiff(string tag){
        return playerDiffs[tag];
    }

    public void IncreasePlayerGold(string tag){
        playerDiffs[tag].IncreaseGold();
    }

    public void IncreasePlayerGoldIncreased(int goldIncrease, string tag){
        playerDiffs[tag].goldIncrease = goldIncrease;
    }
    
    public void BuildingDestroyed(Building buildingToDestroy){
        string playerTag = buildingToDestroy.gameObject.tag;
        if(buildings.Contains(buildingToDestroy)){
            buildings.Remove(buildingToDestroy);
            int playerBuildings = 0;
            foreach(Building building in buildings){
                if(building.gameObject.CompareTag(playerTag)){
                    playerBuildings++;
                }
            }
            if(playerBuildings == 0)
            {
                PlayerDeath(playerTag);
            }
        }

    }

    private void PlayerDeath(string playerTag)
    {
        if (alivePlayerTags.Contains(playerTag))
        {
            alivePlayerTags.Remove(playerTag);
        }
        if(alivePlayerTags.Count > 1){
            if(currentPlayerTag == playerTag){
                FindObjectOfType<UpperUi>().pauseMenu.Pause(false, false);
            }
        }
        else{
            if(alivePlayerTags.Contains(currentPlayerTag)){
                FindObjectOfType<UpperUi>().pauseMenu.Pause(true, true);
            }
            else{
                FindObjectOfType<UpperUi>().pauseMenu.Pause(true, false);
            }
        }
    }

    private void CreateTagToRace(){
        tagToRace = new Dictionary<string, string>();
        tagToRace.Add("Player 1", "Blue");
        tagToRace.Add("Player 2", "Red");
        tagToRace.Add("Player 3", "Black");
        tagToRace.Add("Player 4", "Green");
    }
    public void AddHero(string playerTag, string uniqueName, Targetable hero){
        hero.onDeath += RemoveHero;
        if(playerHeroesOnMap == null){
            playerHeroesOnMap = new Dictionary<string, Dictionary<string, Targetable>>();
        }
        if(!playerHeroesOnMap.ContainsKey(playerTag)){
            playerHeroesOnMap.Add(playerTag, new Dictionary<string, Targetable>());
        }
        if(playerHeroesOnMap[playerTag] == null){
            playerHeroesOnMap[playerTag] = new Dictionary<string, Targetable>();
        }
        playerHeroesOnMap[playerTag].Add(uniqueName, hero);
    }
    public void RemoveHero(Targetable target, Attacking attacker){
        string playerTag = target.gameObject.tag;
        if(playerHeroesOnMap.ContainsKey(playerTag)){
            if(playerHeroesOnMap[playerTag].ContainsValue(target)){
                string desiredKey = "";
                foreach(string key in playerHeroesOnMap[playerTag].Keys){
                    if(playerHeroesOnMap[playerTag][key] == target){
                        desiredKey = key;
                    }
                }
                playerHeroesOnMap[playerTag].Remove(desiredKey);
            }
        }
    }
    public bool IsHeroOnMap(string playerTag, string uniqueName){
        if(playerHeroesOnMap != null && playerHeroesOnMap.ContainsKey(playerTag) && playerHeroesOnMap[playerTag] != null){
            return playerHeroesOnMap[playerTag].ContainsKey(uniqueName);
        }
        else{
            return false;
        }
    }
}
