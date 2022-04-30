using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromptSystem : MonoBehaviour
{
    float startTime;

    [SerializeField]
    GameObject weaponsKeyPromptPrefab;
    GameObject weaponsKeyPromptObj;

    [SerializeField]
    GameObject movementKeyPromptPrefab;
    GameObject movementKeyPromptObj;

    [SerializeField]
    GameObject movementKeyPromptPrefab2;
    GameObject movementKeyPromptObj2;

    bool hidden;

    void Start()
    {
        startTime = Time.time;

        weaponsKeyPromptObj = Instantiate(weaponsKeyPromptPrefab);
        weaponsKeyPromptObj.transform.parent = transform;
        weaponsKeyPromptObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300f, -75f, 0);
        weaponsKeyPromptObj.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);

        movementKeyPromptObj = Instantiate(movementKeyPromptPrefab);
        movementKeyPromptObj.transform.parent = transform;
        movementKeyPromptObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(22f, -90f, 0);
        movementKeyPromptObj.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false); 

        movementKeyPromptObj2 = Instantiate(movementKeyPromptPrefab2);
        movementKeyPromptObj2.transform.parent = transform;
        movementKeyPromptObj2.GetComponent<RectTransform>().anchoredPosition = new Vector3(120f, 0f, 0);
        movementKeyPromptObj2.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false); 
    }

    private void Update()
    {
        if (Time.time - startTime > 10f && !hidden)
        {
            HideControlTips();
        }
    }

    void HideControlTips()
    {
        StartCoroutine(FadeOut(weaponsKeyPromptObj, weaponsKeyPromptObj.GetComponent<Elements>().elements));
        StartCoroutine(FadeOut(movementKeyPromptObj, movementKeyPromptObj.GetComponent<Elements>().elements));
        StartCoroutine(FadeOut(movementKeyPromptObj2, movementKeyPromptObj2.GetComponent<Elements>().elements));
        hidden = true;
    }

    IEnumerator FadeOut(GameObject obj, GameObject[] elements)
    {
        Color color;

        float time = 0f;
        while (time < 3f)
        {
            foreach (GameObject element in elements)
            {
                if (element.TryGetComponent<Image>(out Image image))
                {
                    color = image.color;
                    if (color.a >= 0.01f)
                    {
                        color.a -= 0.01f;
                    }
                    image.color = color;
                }

                if (element.TryGetComponent<Text>(out Text text))
                {
                    color = text.color;
                    if (color.a >= 0.01f)
                    {
                        color.a -= 0.01f;
                    }
                    text.color = color;
                }

                if (element.TryGetComponent<RawImage>(out RawImage rawImage))
                {
                    color = rawImage.color;
                    if (color.a >= 0.01f)
                    {
                        color.a -= 0.01f;
                    }
                    rawImage.color = color;
                }
            }
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(obj);
    }
}
