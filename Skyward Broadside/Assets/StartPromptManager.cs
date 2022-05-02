using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPromptManager : MonoBehaviour
{
    public GameObject promptPrefab;
    public GameObject promptObj;

    GameObject[] keyBinds;
    KeyCodeConverter kcc;

    public KeyCode[] keyCodes;
    public Vector3 anchoredPos;

    private void Awake()
    {
        kcc = gameObject.AddComponent<KeyCodeConverter>();
    }

    public void MakePrompt()
    {
        promptObj = Instantiate(promptPrefab);
        keyBinds = promptObj.GetComponent<Elements>().keyBinds;
        SetKeys();
        promptObj.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
        promptObj.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
    }

    void SetKeys()
    {
        int kk = 0;
        foreach (KeyCode kc in keyCodes)
        {
            keyBinds[kk].GetComponent<Text>().text = kcc.keycodes[kc];
            kk++;
        }
    }

    public void DestroyPrompt()
    {
        Destroy(gameObject);
    }
}
