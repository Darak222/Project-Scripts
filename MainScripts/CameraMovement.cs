using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Vector2 limitZ;
    public Vector2 limitX;
    public Vector2 limitZoom; 
    public float cameraSpeed;
    public float zoomSpeed;
    public Vector2 player1StartPoint;
    public Vector2 player2StartPoint;
    public Vector2 player3StartPoint;
    public Vector2 player4StartPoint;
    private Camera mainCamera;

    void Start(){
        mainCamera = Camera.main;
    }

    void Update()
    {
        float zoom = Input.mouseScrollDelta.y;
        if(zoom != 0){
            mainCamera.fieldOfView += zoomSpeed * zoom * Time.deltaTime;
            if(mainCamera.fieldOfView > limitZoom.x){
                mainCamera.fieldOfView = limitZoom.x;
            }
            if(mainCamera.fieldOfView < limitZoom.y){
                mainCamera.fieldOfView = limitZoom.y;
            }
        }
        if(Input.GetKey(KeyCode.W)){
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + Time.deltaTime*cameraSpeed);
            if(transform.position.z > limitZ.x){
                transform.position = new Vector3(transform.position.x, transform.position.y, limitZ.x);
            }
        }
        if(Input.GetKey(KeyCode.S)){
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - Time.deltaTime*cameraSpeed);
            if(transform.position.z < limitZ.y){
                transform.position = new Vector3(transform.position.x, transform.position.y, limitZ.y);
            }
        }
        if(Input.GetKey(KeyCode.A)){
            transform.position = new Vector3(transform.position.x - Time.deltaTime*cameraSpeed, transform.position.y, transform.position.z);
            if(transform.position.x < limitX.y){
                transform.position = new Vector3(limitX.y, transform.position.y, transform.position.z);
            }
        }
        if(Input.GetKey(KeyCode.D)){
            transform.position = new Vector3(transform.position.x + Time.deltaTime*cameraSpeed, transform.position.y, transform.position.z);
            if(transform.position.x > limitX.x){
                transform.position = new Vector3(limitX.x, transform.position.y, transform.position.z);
            }
        }
    }

    public void SetStartingPosition(string playerTag){
        if(mainCamera == null){
            mainCamera = Camera.main;
        }
        switch(playerTag){
            case "Player 1":
                mainCamera.transform.position = new Vector3(player1StartPoint.x, transform.position.y, player1StartPoint.y);
                break;
            case "Player 2":
                mainCamera.transform.position = new Vector3(player2StartPoint.x, transform.position.y, player2StartPoint.y);;
                break;
            case "Player 3":
                mainCamera.transform.position = new Vector3(player3StartPoint.x, transform.position.y, player3StartPoint.y);;
                break;
            case "Player 4":
                mainCamera.transform.position = new Vector3(player4StartPoint.x, transform.position.y, player4StartPoint.y);;
                break;
        }
    }
}
