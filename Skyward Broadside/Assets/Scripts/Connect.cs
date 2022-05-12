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

    // Join the lobby once connected to photon
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    // Disconnects and reconnects to photon
    public void RejoinLobby()
    {
        PhotonNetwork.Disconnect();
        Start();
    }

    // Sets the room to go to once the team and ship are chosen
    public static void LaunchWithRoom(string name)
    {
        PlayerPrefs.SetString("roomName", name);
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("ChooseTeam");
    }
}
