using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

//A way to globally expose information when there is only one instance that you want to care about
//Rather than finding the player and navigating the hirarchy, you can just get data here, for example
public static class Blackboard
{
    static public PlayerPhotonHub playerPhotonHub;
    static public GameManager gameManager;
    static public GameObject shipCamera;
    static public GameObject lockOnCamera;
    static public List<GameObject> yellowReloadObjects = new List<GameObject>();
    static public List<GameObject> purpleReloadObjects = new List<GameObject>();

    static public List<BreakMaster> breakMasters = new List<BreakMaster>();


    static Dictionary<float, GameObject> playersByID = new Dictionary<float, GameObject>();
}
