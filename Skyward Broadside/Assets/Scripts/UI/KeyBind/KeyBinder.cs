using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyBinder : MonoBehaviour
{
    SBControls.ControlCode toChange;
    Text textToChange;
    string lastText = "";

    bool binding = false;

    //Given to a button element
    public void OnClick(KeyBindChild child)
    {
        //Reset the current option
        if(binding == true)
        {
            textToChange.text = lastText;
        }

        binding = true;
        toChange = child.toChange;
        Debug.LogWarning("Now binding " + child.toChange);
        textToChange = child.textToSet;
        lastText = textToChange.text;
        textToChange.text = "?";
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey && binding)
        {
            binding = false;
            SBControls.SetControlTo(toChange, e.keyCode);
            Debug.LogWarning("Bound to " + e.keyCode);
            textToChange.text = e.keyCode.ToString();
        }
    }
}
