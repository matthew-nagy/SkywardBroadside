using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Connect : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void RejoinLobby()
    {
        PhotonNetwork.Disconnect();
        Start();
    }

    public static void LaunchWithRoom(string name)
    {
        PlayerPrefs.SetString("roomName", name);
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Launcher");
    }
}
