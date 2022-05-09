using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MousePromptManager : MonoBehaviour
{
    [SerializeField]
    GameObject mousePromptPrefab;

    public string promptText;

    public GameObject target;

    GameObject mousePromptObj;

    public void MakePrompt()
    {
        mousePromptObj = Instantiate(mousePromptPrefab);
        mousePromptObj.GetComponent<MousePrompt>().keyPromptText.text = promptText;
        mousePromptObj.GetComponent<MousePrompt>().offset = new Vector3(0f, -25f, 0f);
        mousePromptObj.GetComponent<MousePrompt>().target = target;
    }

    public void DestroyPrompt()
    {
        Destroy(mousePromptObj);
    }

    public void UpdatePrompt()
    {
        if (mousePromptObj != null)
        {
            mousePromptObj.GetComponent<MousePrompt>().keyPromptText.text = promptText;
        }
    }
}
