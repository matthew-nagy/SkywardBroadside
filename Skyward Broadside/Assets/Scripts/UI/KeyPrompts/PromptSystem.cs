using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PromptSystem : MonoBehaviour
{
    float startTime;

    [SerializeField]
    GameObject introManager;

    [SerializeField]
    GameObject weaponsKeyPromptPrefab;
    GameObject weaponsPM;

    [SerializeField]
    GameObject movementKeyPromptPrefab;
    GameObject movementPM;

    [SerializeField]
    GameObject movementKeyPromptPrefab2;
    GameObject movementPM2;

    [SerializeField]
    GameObject scoreboardKeyPromptPrefab;
    GameObject scoreboardPM;

    [SerializeField]
    GameObject promptManagerPrefab;

    [SerializeField]
    GameObject startPromptManagerPrefab;

    GameObject resupplyBase;

    GameObject[] keyBinds;

    KeyCodeConverter kcc;

    bool tipsShown;

    bool pressedWASD;
    bool pressedRF;
    bool pressedAmmo;
    bool pressedTab;

    bool weaponsTipHidden;
    bool WASDTipHidden;
    bool RFTipHidden;
    bool scoreboardTipHidden;

    private void Awake()
    {
        kcc = gameObject.AddComponent<KeyCodeConverter>();
    }

    private void Update()
    {
        if (introManager.GetComponent<Intro>().introDone)
        {
            if (!tipsShown)
            {
                startTime = Time.time;
                if (transform.root.GetChild(0).GetChild(0).GetComponent<PhotonView>().IsMine)
                {
                    GameObject resupplyPromptManager = Instantiate(promptManagerPrefab);
                    resupplyPromptManager.transform.parent = transform;
                    resupplyPromptManager.GetComponent<PromptManager>().promptText = "Stay within the deflector shield to resupply";
                    resupplyPromptManager.GetComponent<PromptManager>().offset = new Vector3(0f, 30f, 0f);
                    resupplyPromptManager.GetComponent<PromptManager>().owner = transform.root.Find("Ship").GetChild(0).gameObject;

                    GameObject[] bases = GameObject.FindGameObjectsWithTag("ResupplyBase");
                    foreach (GameObject _base in bases)
                    {
                        if ((int)_base.GetComponent<ReloadRegister>().myTeam == (int)transform.root.GetComponent<PlayerPhotonHub>().myTeam)
                        {
                            resupplyPromptManager.GetComponent<PromptManager>().target = _base;
                            resupplyPromptManager.GetComponent<PromptManager>().MakePrompt();
                        }
                    }
                }

                weaponsPM = Instantiate(startPromptManagerPrefab);
                weaponsPM.transform.parent = transform;
                StartPromptManager weaponsSPM = weaponsPM.GetComponent<StartPromptManager>();
                weaponsSPM.promptPrefab = weaponsKeyPromptPrefab;
                weaponsSPM.keyCodes = new KeyCode[] { SBControls.ammo1.primaryKey, SBControls.ammo2.primaryKey, SBControls.ammo3.primaryKey };
                weaponsSPM.anchoredPos = new Vector3(-300f, -75f, 0f);
                weaponsSPM.MakePrompt();

                movementPM = Instantiate(startPromptManagerPrefab);
                movementPM.transform.parent = transform;
                StartPromptManager movementSPM = movementPM.GetComponent<StartPromptManager>();
                movementSPM.promptPrefab = movementKeyPromptPrefab;
                movementSPM.keyCodes = new KeyCode[] { SBControls.left.primaryKey, SBControls.backwards.primaryKey, SBControls.right.primaryKey, SBControls.forwards.primaryKey };
                movementSPM.anchoredPos = new Vector3(22f, -90f, 0f);
                movementSPM.MakePrompt();


                movementPM2 = Instantiate(startPromptManagerPrefab);
                movementPM2.transform.parent = transform;
                StartPromptManager movementSPM2 = movementPM2.GetComponent<StartPromptManager>();
                movementSPM2.promptPrefab = movementKeyPromptPrefab2;
                movementSPM2.keyCodes = new KeyCode[] { SBControls.yAxisDown.primaryKey, SBControls.yAxisUp.primaryKey };
                movementSPM2.anchoredPos = new Vector3(120f, 0f, 0f);
                movementSPM2.MakePrompt();

                scoreboardPM = Instantiate(startPromptManagerPrefab);
                scoreboardPM.transform.parent = transform;
                StartPromptManager scoreboardSPM = scoreboardPM.GetComponent<StartPromptManager>();
                scoreboardSPM.promptPrefab = scoreboardKeyPromptPrefab;
                scoreboardSPM.keyCodes = new KeyCode[] { SBControls.viewScoreboard.primaryKey };
                scoreboardSPM.anchoredPos = new Vector3(-300f, 100f, 0f);
                scoreboardSPM.MakePrompt();

                tipsShown = true;
            }

            //hide wasd tips if one of the wasd bound keys is pressed
            if (!WASDTipHidden)
            {
                if (SBControls.forwards.IsDown())
                {
                    pressedWASD = true;
                }
                if (SBControls.backwards.IsDown())
                {
                    pressedWASD = true;
                }
                if (SBControls.left.IsDown())
                {
                    pressedWASD = true;
                }
                if (SBControls.right.IsDown())
                {
                    pressedWASD = true;
                }
            }

            //hide rf tips if one of the rf bound keys is pressed
            if (!RFTipHidden)
            {
                if (SBControls.yAxisUp.IsDown())
                {
                    pressedRF = true;
                }
                if (SBControls.yAxisDown.IsDown())
                {
                    pressedRF = true;
                }
            }


            //hide ammo tips if one of the ammo bound keys is pressed
            if (!weaponsTipHidden)
            {
                if (SBControls.ammo1.IsDown())
                {
                    pressedAmmo = true;
                }
                if (SBControls.ammo2.IsDown())
                {
                    pressedAmmo = true;
                }
                if (SBControls.ammo3.IsDown())
                {
                    pressedAmmo = true;
                }
            }

            if (!scoreboardTipHidden)
            {
                if (SBControls.viewScoreboard.IsDown())
                {
                    pressedTab = true;
                }
            }

            //hide all start tips after a time
            if (Time.time - startTime >= 30f)
            {
                pressedWASD = true;
                pressedRF = true;
                pressedAmmo = true;
                pressedTab = true;
            }

            //hide tips if told to
            if (pressedWASD && !WASDTipHidden)
            {
                HideWASDTip();
                WASDTipHidden = true;
            }

            if (pressedRF && !RFTipHidden)
            {
                HideRFTip();
                RFTipHidden = true;
            }

            if (pressedAmmo && !weaponsTipHidden)
            {
                HideWeaponsTip();
                weaponsTipHidden = true;
            }

            if (pressedTab && !scoreboardTipHidden)
            {
                HideScoreboardTip();
                scoreboardTipHidden = true;
            }
        }
    }

    void HideWeaponsTip()
    {
        StartCoroutine(FadeOut(weaponsPM.GetComponent<StartPromptManager>().promptObj, weaponsPM.GetComponent<StartPromptManager>().promptObj.GetComponent<Elements>().elements));
    }

    void HideWASDTip()
    {
        StartCoroutine(FadeOut(movementPM.GetComponent<StartPromptManager>().promptObj, movementPM.GetComponent<StartPromptManager>().promptObj.GetComponent<Elements>().elements));
    }

    void HideRFTip()
    {
        StartCoroutine(FadeOut(movementPM2.GetComponent<StartPromptManager>().promptObj, movementPM2.GetComponent<StartPromptManager>().promptObj.GetComponent<Elements>().elements));
    }

    void HideScoreboardTip()
    {
        StartCoroutine(FadeOut(scoreboardPM.GetComponent<StartPromptManager>().promptObj, scoreboardPM.GetComponent<StartPromptManager>().promptObj.GetComponent<Elements>().elements));
    }

    //gradually reduce alpha value of tip to fade it out
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
