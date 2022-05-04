using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromptManager : MonoBehaviour
{
    [SerializeField]
    GameObject promptPrefab;

    public string promptText;

    public GameObject owner;
    public GameObject target;

    GameObject promptObj;

    public Vector3 offset;

    bool visible;
    bool active;

    [SerializeField]
    LayerMask layerMask;

    public void MakePrompt()
    {
        promptObj = Instantiate(promptPrefab, transform.position + new Vector3(0f, 10000f, 0f), Quaternion.identity);
        promptObj.GetComponent<Prompt>().promptText.text = promptText;
        promptObj.GetComponent<Prompt>().offset = offset;
        promptObj.GetComponent<Prompt>().target = target;
        promptObj.GetComponent<Prompt>().owner = owner;
        active = true;
    }

    public void DestroyPrompt()
    {
        Destroy(promptObj);
        active = false;
    }

    public void UpdatePrompt()
    {
        promptObj.GetComponent<Prompt>().promptText.text = promptText;
    }

    private void Update()
    {
        CheckVisible();
        if (visible)
        {
            if (!active)
            {
                MakePrompt();
            }
        }
        else
        {
            if (active)
            {
                DestroyPrompt();
            }
        }
    }

    void CheckVisible()
    {
        Vector3 screenPoint = Camera.main.gameObject.GetComponent<Camera>().WorldToViewportPoint(target.transform.position);
        if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
        {
            RaycastHit hit;
            if (Physics.Linecast(start: owner.transform.position, end: target.transform.position, hitInfo: out hit, layerMask: layerMask))
            {
                if (hit.collider.gameObject == target)
                {
                    visible = true;
                }
                else
                {
                    visible = false;
                }
            }
        }
        else
        {
            visible = false;
        }
    }
}
