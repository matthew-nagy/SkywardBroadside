using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required when Using UI elements

//Used for UI sliders to ping the camera controller once a value changes
public class MouseSensitivity : MonoBehaviour
{
    public Slider mySlider;

    // Start is called before the first frame update
    void Start()
    {
        CameraController.sensitivitySlider = mySlider;
        mySlider.value = CameraController.sensitivity;
    }

    //When the value of the slider changed, ping the static camera controller instance
    public void OnValueChange()
    {
        CameraController.SetSensitivity();
    }
}
