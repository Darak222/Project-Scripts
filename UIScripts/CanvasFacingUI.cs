using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasFacingUI : MonoBehaviour
{

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        this.transform.rotation = mainCam.transform.rotation;
    }
}
