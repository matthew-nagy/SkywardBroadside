//Script for an ingame prompt with a mouse icon. Setting its position, size, image etc.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MousePrompt : MonoBehaviour
{
    public Text keyPromptText;

    public GameObject imageObj;

    public GameObject background;

    public RawImage keyImage;

    public Vector3 offset;

    public GameObject target;

    private void Awake()
    {
        transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
    }

    private void Update()
    {
        if (target != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(target.transform.position) + offset;
            imageObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(keyPromptText.text.Length * -3.65f, 0f, 0f);
            background.GetComponent<RectTransform>().anchoredPosition = new Vector3(keyPromptText.text.Length * -0.54f, 0f, 0f);
            background.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, keyPromptText.text.Length * 7.77f);
        }
    }
}