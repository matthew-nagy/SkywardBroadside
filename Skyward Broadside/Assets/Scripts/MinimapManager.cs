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

    // Start is called before the first frame update
    void Start()
    {
        int teamNo = (int)transform.root.Find("Ship").Find(PlayerChoices.ship).GetComponent<PlayerController>().myTeam;
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
    }
}
