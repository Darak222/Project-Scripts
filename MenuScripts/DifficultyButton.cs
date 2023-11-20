using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour
{
    public Text selectedDifficulty;
    public Button startGameButton;
    public void SelectDifficulty(int startingGold){
        MenuManager.instance.SelectDifficulty(startingGold);
    }
    public void ChangeText(string difficulty){
        selectedDifficulty.text = "You have selected <b>"+difficulty.ToUpper()+ "</b> difficulty";
        startGameButton.interactable = true;
    }
    public void StartGame(){
        MenuManager.instance.StartGame();
    }
}
