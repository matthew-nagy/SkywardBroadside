using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Prompt : MonoBehaviour
{
    public Text promptText;

    public GameObject background;

    public Vector3 offset;

    public GameObject owner;
    public GameObject target;

    private void Awake()
    {
        transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
    }

    private void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(target.transform.position + offset);
        background.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, promptText.text.Length * 7.77f);
        promptText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, promptText.text.Length * 7.77f);
    }
}