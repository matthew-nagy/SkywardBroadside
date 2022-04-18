using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields

    /// <summary>
    /// The maximum number of players per room. When a room is full, it can't be joined by new players
    /// and so new room will be created
    /// </summary>
    [Tooltip(
        "The maximum number of players per room. When a room is full, it can't be joined by new player and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 10;

    [Tooltip("The UI Panel to let the user enter name, connect and play")]
    [SerializeField]
    private GameObject controlPanel;

    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField]
    private GameObject progressLabel;

    private string roomName = "GameWorld";

#endregion

    #region Private Fields

    /// <summary>
    /// This client's version number. Users are separated from eachother by gameVersion
    /// (which allows you to make breaking changes)
    /// </summary>
    private string gameVersion = "1";

    /// <summary>
    /// Keep track of current progress. Since connection is async and is based on several callbacks from Photon,
    /// we need to keep track of this to properly adjust the behaviour when we receive call back by Photon.
    /// Typpically this is used for OnConnectedToMaster() callback
    /// </summary>
    private bool isConnecting;

    #endregion
    
    #region MonoBehaviour CallBacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// <summary>
    void Awake()
    {
        // #Critical
        // This makes sure we can use PhotonNetwork.LoadLevel() on the master client
        // and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);

        Random.InitState((int)System.DateTime.Now.Ticks);
        PlayerPrefs.SetFloat("UUID", Random.Range(float.MinValue, float.MaxValue));
    }
    
    #endregion
    
    #region Public Methods

    /// <summary>
    /// Start the connection process.
    /// - If already connected, we attempt joining a randomr oom
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>

    public void Connect()
    {
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);
        
        // we check if we are connected or not, we join if we are, else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed
            PhotonNetwork.GameVersion = gameVersion;
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = maxPlayersPerRoom;
            Debug.Log(PlayerPrefs.GetString("roomName"));
            PhotonNetwork.JoinOrCreateRoom(PlayerPrefs.GetString("roomName"), options, TypedLobby.Default);
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            // keep track of the will to join a room because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            PhotonNetwork.GameVersion = gameVersion;
            isConnecting = PhotonNetwork.ConnectUsingSettings();
        }
    }
    
    #endregion
    
    #region MonoBehaviourPunCallbacks callbacks

    public override void OnConnectedToMaster()
    {

        // We don't want to do anything if we are not attempting to join a room.
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isConnecting)
        {
            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else
            // we'll be called back with OnJoinRandomFailed()
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = maxPlayersPerRoom;
            Debug.Log(PlayerPrefs.GetString("roomName"));
            PhotonNetwork.JoinOrCreateRoom(PlayerPrefs.GetString("roomName"), options, TypedLobby.Default);
            isConnecting = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);

        isConnecting = false;
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            // Load the world
            PhotonNetwork.LoadLevel(roomName);
        }
    }
    
    #endregion

    }
