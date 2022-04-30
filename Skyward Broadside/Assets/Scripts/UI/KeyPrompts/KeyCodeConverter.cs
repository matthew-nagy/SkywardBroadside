using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class KeyCodeConverter : MonoBehaviour
{
    public Dictionary<KeyCode, string> keycodes = new Dictionary<KeyCode, string>();

    public bool done;

    private void Awake()
    {
        foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
        {
            if (!keycodes.ContainsKey(k))
            {
                keycodes.Add(k, k.ToString());
            }
        }

        //change the alpha key strings to just the number without alpha in front
        for (int k = 0; k < 10; k++)
        {
            keycodes[(KeyCode)((int)KeyCode.Alpha0 + k)] = k.ToString(); 
        }
        done = true;
    }
}
