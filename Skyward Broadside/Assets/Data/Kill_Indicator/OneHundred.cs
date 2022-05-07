using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class OneHundred : MonoBehaviour
{
    public string playerName;

    [SerializeField]
    GameObject background;
    [SerializeField]
    GameObject playerNameText;

    private void Awake()
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, -100f, 0f);
    }

    public void Show()
    {
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (9 + playerName.Length + 4) * 8);
        playerNameText.GetComponent<Text>().text = playerName;
        transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
    }
}