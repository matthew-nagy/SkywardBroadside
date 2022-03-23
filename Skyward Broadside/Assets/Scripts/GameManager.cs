using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    // red and blue player spawns to distinguish between teams
    public GameObject redSpawn;
    public GameObject blueSpawn;

    //The player photon hub for the person playing this instance
    public PlayerPhotonHub serverPlayerPhotonHub;

    
    #region Photon Callbacks

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher  scene
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    
    #endregion
    
    #region Public Methods

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    
    #endregion
    
    #region Private Methods

    void Start()
    {
        Instance = this;
        Blackboard.gameManager = this;

        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> ship prefab reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (PlayerManager.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                if (TeamButton.joinTeam == "Red")
                {
                    Vector3 spawnPoint = redSpawn.transform.position + new Vector3(Random.Range(-80, 80), 0, Random.Range(-80, 80));
                    GameObject newPlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, spawnPoint, Quaternion.identity, 0);
                    newPlayer.GetComponent<PlayerPhotonHub>().SetTeam(1);
                    newPlayer.transform.Find("Ship").GetComponent<PlayerController>().myTeam = 0;
                    if (PhotonNetwork.LocalPlayer.GetPhotonTeam() == null)
                    {
                        PhotonNetwork.LocalPlayer.JoinTeam("Red");
                        Debug.Log(PhotonNetwork.LocalPlayer.GetPhotonTeam());
                    }
                    else
                    {
                        PhotonNetwork.LocalPlayer.SwitchTeam("Red");
                        Debug.Log(PhotonNetwork.LocalPlayer.GetPhotonTeam());
                    }
                    Debug.Log("Joined");
                }
                else if (TeamButton.joinTeam == "Blue")
                {
                    Vector3 spawnPoint = blueSpawn.transform.position + new Vector3(Random.Range(-80, 80), 0, Random.Range(-80, 80));
                    GameObject newPlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, spawnPoint, Quaternion.identity, 0);
                    newPlayer.GetComponent<PlayerPhotonHub>().SetTeam(0);
                    newPlayer.transform.Find("Ship").GetComponent<PlayerController>().myTeam = 1;
                    Debug.Log(spawnPoint);
                    if (PhotonNetwork.LocalPlayer.GetPhotonTeam() == null)
                    {
                        PhotonNetwork.LocalPlayer.JoinTeam("Blue");
                        Debug.Log(PhotonNetwork.LocalPlayer.GetPhotonTeam());
                    }
                    else
                    {
                        PhotonNetwork.LocalPlayer.SwitchTeam("Blue");
                        Debug.Log(PhotonNetwork.LocalPlayer.GetPhotonTeam());
                    }
                    //Debug.Log("Joined");
                }
                
                //Debug.Log(PhotonNetwork.LocalPlayer.GetPhotonTeam());
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            setRoomStartTimeAndInitialScores();
        }
    }

    private void Update()
    {

    }

    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork: Trying to load a level but we are not the master client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : GameWorld");
        PhotonNetwork.LoadLevel("GameWorld");
    }

    void setRoomStartTimeAndInitialScores()
    {
        DateTime currentTime = System.DateTime.Now;
       
        Hashtable properties = new Hashtable();
        properties.Add("startTime", currentTime.ToString());
        int[] scores = {0, 0};
        properties.Add("scores", scores);
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    #endregion
    
    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
        }
    }
        
    #endregion Photon Callbacks

}