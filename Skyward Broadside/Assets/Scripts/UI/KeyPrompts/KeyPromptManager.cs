//Manage a prompt with a key icon. Set its text and position

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyPromptManager : MonoBehaviour
{
    [SerializeField]
    GameObject keyPromptPrefab;

    public string promptText;

    public Vector3 pos;

    GameObject keyPromptObj;

    public void MakePrompt()
    {
        keyPromptObj = Instantiate(keyPromptPrefab);
        keyPromptObj.GetComponent<KeyPrompt>().keyPromptText.text = promptText;
        keyPromptObj.GetComponent<KeyPrompt>().pos = pos;
    }

    public void DestroyPrompt()
    {
        Destroy(keyPromptObj);
    }

    public void UpdatePrompt()
    {
        keyPromptObj.GetComponent<MousePrompt>().keyPromptText.text = promptText;
    }
}
