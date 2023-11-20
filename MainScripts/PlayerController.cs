using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{

    Camera mainCam;
    void Start()
    {
        mainCam = Camera.main;

    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            FindNewTarget();
        }
        if(Input.GetKeyDown(KeyCode.UpArrow)){
            if(Time.timeScale + 1 <= 3){
                Time.timeScale += 1;
            }
        }
        if(Input.GetKeyDown(KeyCode.DownArrow)){
            if(Time.timeScale - 1 >= 1){
                Time.timeScale -= 1;
            }
        }

    }

    private void FindNewTarget()
    {
        if(!EventSystem.current.IsPointerOverGameObject()){
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);
            List<RaycastHit> targetablesHit = new List<RaycastHit>();
            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider.isTrigger)
                {
                    Targetable tmp = hit.transform.GetComponent<Targetable>();
                    if (tmp != null)
                    {
                        targetablesHit.Add(hit);
                    }
                }
            }
            if (targetablesHit.Count > 0)
            {
                float minDistance = float.MaxValue;
                RaycastHit minDistanceObject = targetablesHit[0];
                foreach (RaycastHit target in targetablesHit)
                {
                    if (target.distance < minDistance)
                    {
                        minDistance = target.distance;
                        minDistanceObject = target;
                    }
                }
                Targetable sendObjectInfo = minDistanceObject.transform.GetComponent<Targetable>();
                ChangeTarget(sendObjectInfo);
            }
        }
    }

    public void ChangeTarget(Targetable sendObjectInfo)
    {
        GameplayManager.instance.bottomUi.statsUi.ChangeTarget(sendObjectInfo);
        GameplayManager.instance.bottomUi.upgradesUi.ChangeTarget(sendObjectInfo);
        GameplayManager.instance.bottomUi.upgradeQueueUi.ChangeTarget(sendObjectInfo);
        //GameplayManager.instance.ChangeCurrentPlayer(sendObjectInfo.tag);
    }
}
