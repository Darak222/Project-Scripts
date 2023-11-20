using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public Button pauseButton;
    public Text victoryInfo;
    public void Pause(bool isEnd, bool isWin = true){
        if(isEnd){
            pauseButton.gameObject.SetActive(false);
            if(isWin){
                victoryInfo.text = "YOU WIN!";
            }
            else{
                victoryInfo.text = "DEFEAT!";
            }
            victoryInfo.gameObject.SetActive(true);
        }
        else{
            pauseButton.gameObject.SetActive(true);
            if(isWin){
                pauseButton.GetComponentInChildren<Text>().text = "RESUME";
            }
            else{
                pauseButton.GetComponentInChildren<Text>().text = "SPECTATE";
            }
            victoryInfo.gameObject.SetActive(false);
        }
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }
    public void Resume(){
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
    public void BackToMenu(){
        SceneManager.LoadScene("Menu");
    }
    public void QuitGame(){
        Application.Quit();
    }
}
