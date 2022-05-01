using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromptManager : MonoBehaviour
{
    [SerializeField]
    GameObject promptPrefab;

    public string promptText;

    public GameObject target;

    GameObject promptObj;

    public Vector3 offset;

    public void MakePrompt()
    {
        promptObj = Instantiate(promptPrefab);
        promptObj.GetComponent<Prompt>().promptText.text = promptText;
        promptObj.GetComponent<Prompt>().offset = offset;
        promptObj.GetComponent<Prompt>().target = target;
    }

    public void DestroyPrompt()
    {
        Destroy(promptObj);
    }

    public void UpdatePrompt()
    {
        promptObj.GetComponent<Prompt>().promptText.text = promptText;
    }
}
