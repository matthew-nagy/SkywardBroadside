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
    private byte maxPlayersPerRoom = 4;
    
    #endregion
    
    #region Private Fields

    /// <summary>
    /// This client's version number. Users are separated from eachother by gameVersion
    /// (which allows you to make breaking changes)
    /// </summary>
    private string gameVersion = "1";

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
        // we check if we are connected or not, we join if we are, else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }
    
    #endregion
    
    #region MonoBehaviourPunCallbacks callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorials/Launcher: OnConnectedToMaster() was called by PUN");
        
        // #Critical: The first we try to do is to join a potential existing room. If there is, good, else
        // we'll be called back with OnJoinRandomFailed()
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
        
        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
    }
    
    #endregion

    }
