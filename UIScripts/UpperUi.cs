using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpperUi : MonoBehaviour
{

    public Text gold;
    public Text timer;
    public PauseMenu pauseMenu; 
    void Start()
    {
        
    }

    void Update()
    {
        gold.text = GameplayManager.instance.GetCurrentPlayerGold().ToString();
        timer.text = string.Format("{0}:{1:00}", Mathf.Floor(GameplayManager.instance.timer / 60), Mathf.Floor(GameplayManager.instance.timer % 60));
    }

    public void PauseGame(){
        pauseMenu.Pause(false);
    }
}
