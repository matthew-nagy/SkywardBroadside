using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

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

    static public GameObject getPlayerByID(float id)
    {
        return playersByID[id];
    }
    static public void registerPlayer(GameObject player, float id)
    {
        playersByID[id] = player;
    }
}
