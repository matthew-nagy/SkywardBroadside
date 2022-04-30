using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyBinder : MonoBehaviour
{
    SBControls.ControlCode toChange;
    Text textToChange;

    bool binding = false;

    //Given to a button element
    public void OnClick(KeyBindChild child)
    {
        binding = true;
        toChange = child.toChange;
        Debug.LogWarning("Now binding " + child.toChange);
        textToChange = child.textToSet;
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
