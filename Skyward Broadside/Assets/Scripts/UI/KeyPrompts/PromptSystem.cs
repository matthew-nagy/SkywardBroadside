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

    [SerializeField]
    GameObject scoreboardKeyPromptPrefab;
    GameObject scoreboardKeyPromptObj;

    [SerializeField]
    GameObject promptManagerPrefab;

    GameObject resupplyBase;

    GameObject[] keyBinds;

    KeyCodeConverter kcc;

    bool pressedForward;
    bool pressedBackward;
    bool pressedLeft;
    bool pressedRight;
    bool pressedUp;
    bool pressedDown;
    bool pressedAmmo1;
    bool pressedAmmo2;
    bool pressedAmmo3;

    bool weaponsTipHidden;
    bool WASDTipHidden;
    bool RFTipHidden;
    bool scoreboardTipHidden;

    private void Awake()
    {
        kcc = gameObject.AddComponent<KeyCodeConverter>();
    }

    void Start()
    {
        startTime = Time.time;

        GameObject resupplyPromptManager = Instantiate(promptManagerPrefab);
        resupplyPromptManager.transform.parent = transform;
        resupplyPromptManager.GetComponent<PromptManager>().promptText = "Stay within the deflector shield to resupply";
        resupplyPromptManager.GetComponent<PromptManager>().offset = new Vector3(0f, 30f, 0f);

        GameObject[] bases = GameObject.FindGameObjectsWithTag("ResupplyBase");
        foreach (GameObject _base in bases) 
        {
            if ((int)_base.GetComponent<ReloadRegister>().myTeam == (int)transform.root.GetComponent<PlayerPhotonHub>().myTeam)
            {
                resupplyPromptManager.GetComponent<PromptManager>().target = _base;
                resupplyPromptManager.GetComponent<PromptManager>().MakePrompt();
            }
        }


        weaponsKeyPromptObj = Instantiate(weaponsKeyPromptPrefab);
        keyBinds = weaponsKeyPromptObj.GetComponent<Elements>().keyBinds;
        keyBinds[0].GetComponent<Text>().text = kcc.keycodes[SBControls.ammo1.primaryKey];
        keyBinds[1].GetComponent<Text>().text = kcc.keycodes[SBControls.ammo2.primaryKey];
        keyBinds[2].GetComponent<Text>().text = kcc.keycodes[SBControls.ammo3.primaryKey];
        weaponsKeyPromptObj.transform.parent = transform;
        weaponsKeyPromptObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300f, -75f, 0f);
        weaponsKeyPromptObj.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);

        movementKeyPromptObj = Instantiate(movementKeyPromptPrefab);
        keyBinds = movementKeyPromptObj.GetComponent<Elements>().keyBinds;
        keyBinds[0].GetComponent<Text>().text = kcc.keycodes[SBControls.left.primaryKey];
        keyBinds[1].GetComponent<Text>().text = kcc.keycodes[SBControls.backwards.primaryKey];
        keyBinds[2].GetComponent<Text>().text = kcc.keycodes[SBControls.right.primaryKey];
        keyBinds[3].GetComponent<Text>().text = kcc.keycodes[SBControls.forwards.primaryKey];
        movementKeyPromptObj.transform.parent = transform;
        movementKeyPromptObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(22f, -90f, 0f);
        movementKeyPromptObj.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);

        movementKeyPromptObj2 = Instantiate(movementKeyPromptPrefab2);
        keyBinds = movementKeyPromptObj2.GetComponent<Elements>().keyBinds;
        keyBinds[0].GetComponent<Text>().text = kcc.keycodes[SBControls.yAxisDown.primaryKey];
        keyBinds[1].GetComponent<Text>().text = kcc.keycodes[SBControls.yAxisUp.primaryKey];
        movementKeyPromptObj2.transform.parent = transform;
        movementKeyPromptObj2.GetComponent<RectTransform>().anchoredPosition = new Vector3(120f, 0f, 0f);
        movementKeyPromptObj2.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);

        scoreboardKeyPromptObj = Instantiate(scoreboardKeyPromptPrefab);
        keyBinds = scoreboardKeyPromptObj.GetComponent<Elements>().keyBinds;
        keyBinds[0].GetComponent<Text>().text = kcc.keycodes[SBControls.viewScoreboard.primaryKey];
        scoreboardKeyPromptObj.transform.parent = transform;
        scoreboardKeyPromptObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300f, 100f, 0f);
        scoreboardKeyPromptObj.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
    }

    private void Update()
    {
        if (SBControls.forwards.IsDown())
        {
            pressedForward = true;
        }
        if (SBControls.backwards.IsDown())
        {
            pressedBackward = true;
        }
        if (SBControls.left.IsDown())
        {
            pressedLeft = true;
        }
        if (SBControls.right.IsDown())
        {
            pressedRight = true;
        }
        if (SBControls.yAxisUp.IsDown())
        {
            pressedUp = true;
        }
        if (SBControls.yAxisDown.IsDown())
        {
            pressedDown = true;
        }
        if (SBControls.ammo1.IsDown())
        {
            pressedAmmo1 = true;
        }
        if (SBControls.ammo2.IsDown())
        {
            pressedAmmo2 = true;
        }
        if (SBControls.ammo3.IsDown())
        {
            pressedAmmo3 = true;
        }
        
        if (pressedForward && pressedBackward && pressedLeft && pressedRight && !WASDTipHidden)
        {
            HideWASDTip();
            WASDTipHidden = true;
        }

        if (pressedUp && pressedDown && !RFTipHidden)
        {
            HideRFTip();
            RFTipHidden = true;
        }

        if (pressedAmmo1 && pressedAmmo2 && pressedAmmo3 && !weaponsTipHidden)
        {
            HideWeaponsTip();
            weaponsTipHidden = true;
        }

        if (SBControls.viewScoreboard.IsDown() && !scoreboardTipHidden)
        {
            HideScoreboardTip();
            scoreboardTipHidden = true;
        }
    }

    void HideWeaponsTip()
    {
        StartCoroutine(FadeOut(weaponsKeyPromptObj, weaponsKeyPromptObj.GetComponent<Elements>().elements));
    }

    void HideWASDTip()
    {
        StartCoroutine(FadeOut(movementKeyPromptObj, movementKeyPromptObj.GetComponent<Elements>().elements));
    }

    void HideRFTip()
    {
        StartCoroutine(FadeOut(movementKeyPromptObj2, movementKeyPromptObj2.GetComponent<Elements>().elements));
    }

    void HideScoreboardTip()
    {
        StartCoroutine(FadeOut(scoreboardKeyPromptObj, scoreboardKeyPromptObj.GetComponent<Elements>().elements));
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
