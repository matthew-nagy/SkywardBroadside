using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MinimapManager : MonoBehaviour
{
    [SerializeField]
    Sprite purpleIcon;
    [SerializeField]
    Sprite yellowIcon;

    bool teamSet;

    //Get the team and set the appropiate minimap symbol
    private void Update()
    {
        string shipType = transform.root.Find("Ship").GetChild(0).transform.name;
        if (!teamSet && transform.root.Find("Ship").Find(shipType).GetComponent<PlayerController>().teamSet)
        {
            int teamNo = (int)transform.root.Find("Ship").Find(shipType).GetComponent<PlayerController>().myTeam;
            if (teamNo == 0)
            {
                GetComponent<SpriteRenderer>().sprite = purpleIcon;
            }
            else if (teamNo == 1)
            {
                GetComponent<SpriteRenderer>().sprite = yellowIcon;
            }
            else
            {
                Debug.LogError("Invalid team name");
            }
            teamSet = true;
        }
    }
}
