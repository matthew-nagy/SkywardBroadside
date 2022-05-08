using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required when Using UI elements

public class MouseSensitivity : MonoBehaviour
{
    public Slider mySlider;

    // Start is called before the first frame update
    void Start()
    {
        CameraController.sensitivitySlider = mySlider;
        mySlider.value = CameraController.sensitivity;
    }

    public void OnValueChange()
    {
        CameraController.SetSensitivity();
    }
}
