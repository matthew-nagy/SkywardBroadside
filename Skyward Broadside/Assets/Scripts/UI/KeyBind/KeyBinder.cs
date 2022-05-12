using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Given to the canvas, which will take in a KeyBindChild on a click and change the SBControls class
public class KeyBinder : MonoBehaviour
{
    SBControls.ControlCode toChange;
    Text textToChange;
    string lastText = "";

    //If you have clicked a button, you must now wait to here a button click
    bool binding = false;

    //Given to a button element
    public void OnClick(KeyBindChild child)
    {
        //Reset the current option
        if(binding == true)
        {
            textToChange.text = lastText;
        }

        //Set up for binding a key
        binding = true;
        toChange = child.toChange;
        textToChange = child.textToSet;
        lastText = textToChange.text;
        textToChange.text = "?";
    }

    //When a button is pressed, intercept it to see if you should bind something
    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey && binding)
        {
            binding = false;
            SBControls.SetControlTo(toChange, e.keyCode);
            textToChange.text = e.keyCode.ToString();
        }
    }
}
