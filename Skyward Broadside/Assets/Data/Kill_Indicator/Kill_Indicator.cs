using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;

public class Kill_Indicator : MonoBehaviour
{
    [SerializeField]
    GameObject oneHundred;

    PhotonView pv;
    GameObject indicator;
    public enum EventCode : byte
    {
        DeathEvent = 1,
        UpdatedScores,
        KillEventWithNames,
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void Start()
    {
        Transform ship = transform.root.Find("Ship");
        if (ship != null)
        {
            pv = ship.GetChild(0).GetComponent<PhotonView>();
        }
    }

    private void OnEvent(EventData photonEvent)
    {
        if (pv != null)
        {
            if (pv.IsMine)
            {
                byte eventCode = photonEvent.Code;
                if (eventCode == (byte)EventCode.KillEventWithNames)
                {
                    var names = (string[])photonEvent.CustomData;
                    if (names[0] == PhotonNetwork.NickName)
                    {
                        Debug.Log("Shown");
                        ShowIndicator(names[1]);
                    }
                }
            }
        } 
    }

    void ShowIndicator(string playerKilled)
    {
        indicator = Instantiate(oneHundred);
        indicator.GetComponent<OneHundred>().playerName = playerKilled;
        indicator.GetComponent<OneHundred>().Show();
        Invoke(nameof(HideIndicator), 5f);
    }
    
    void HideIndicator()
    {
        StartCoroutine(nameof(FadeOut));
    }

    IEnumerator FadeOut()
    {
        GameObject[] elements = indicator.GetComponent<Elements>().elements;
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
        Destroy(indicator);
    }
}
