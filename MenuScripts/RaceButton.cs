using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceButton : MonoBehaviour
{
    public Text selectedRace;
    public Button startButton;
    public void SelectRace(string playerTag){
        MenuManager.instance.SelectRace(playerTag);
    }
    public void ChangeText(string playerRace){
        selectedRace.text = "You have selected <b>"+playerRace.ToUpper()+"</b>";
        startButton.interactable = true;
    }
    public void StartGame(){
        MenuManager.instance.StartGame();
    }
    public void ExitGame(){
        MenuManager.instance.QuitGame();
    }
}
