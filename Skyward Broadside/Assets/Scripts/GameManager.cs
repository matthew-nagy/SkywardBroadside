using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using UnityEngine.AI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    // red and blue player prefabs and spawns to distinguish between teams
    public GameObject redPlayerPrefab;
    public GameObject bluePlayerPrefab;
    public GameObject redSpawn;
    public GameObject blueSpawn;
    
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

        if (playerPrefab == null || redPlayerPrefab == null || bluePlayerPrefab == null)
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
                    PhotonNetwork.Instantiate(this.redPlayerPrefab.name, spawnPoint, Quaternion.identity, 0);
                    Debug.Log(spawnPoint);
                    if (PhotonNetwork.LocalPlayer.GetPhotonTeam() == null)
                    {
                       PhotonNetwork.LocalPlayer.JoinTeam("Red");
                    }
                    else
                    {
                        PhotonNetwork.LocalPlayer.SwitchTeam("Red");
                    }
                    Debug.Log("Joined");
                }
                else if (TeamButton.joinTeam == "Blue")
                {
                    Vector3 spawnPoint = blueSpawn.transform.position + new Vector3(Random.Range(-80, 80), 0, Random.Range(-80, 80));
                    PhotonNetwork.Instantiate(this.bluePlayerPrefab.name, spawnPoint, Quaternion.identity, 0);
                    Debug.Log(spawnPoint);
                    if (PhotonNetwork.LocalPlayer.GetPhotonTeam() == null)
                    {
                        PhotonNetwork.LocalPlayer.JoinTeam("Blue");
                    }
                    else
                    {
                        PhotonNetwork.LocalPlayer.SwitchTeam("Blue");
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

    #endregion
    
    #region Photon Callbacks

    // not the behaviour we want, calls LoadArena whenever new player joins?
    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            
            //LoadArena();
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);

            //LoadArena();
        }
    }
        
    #endregion Photon Callbacks

}
