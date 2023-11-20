using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;
    public string chosenPlayerTag;
    public int difficultyGoldStarter;

    void Awake()
    {
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(instance);
        }    
        else{
            DestroyImmediate(this);
        }
    }

    void Update()
    {
        
    }

    public void StartGame(){
        SceneManager.LoadScene("Game");
    }

    public void QuitGame(){
        Application.Quit();
    }

    public void SelectRace(string playerTag){
        chosenPlayerTag = playerTag;
    }
    public void SelectDifficulty(int startingGold){
        difficultyGoldStarter = startingGold;
    }
}
