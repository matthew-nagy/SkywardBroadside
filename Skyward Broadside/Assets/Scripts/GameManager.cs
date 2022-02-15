using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using UnityEngine.AI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    
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
        print("Leaving Room");
    }
    
    #endregion
    
    #region Private Methods

    void Start()
    {
        Instance = this;

        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> ship prefab reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (PlayerManager.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
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
