using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuiUpdateScript : MonoBehaviour
{
    public Text health;

    public void UpdateGUIHealth(float healthVal)
    {
        // This will at some point have some complicated extra stuff for a more interesting GUI i.e. dial control
        // but this is simple atm
        health.text = healthVal.ToString();
    }

}
