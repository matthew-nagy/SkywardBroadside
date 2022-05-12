using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Do not use this to go forward to ship screen. This is only used to go back from the lobby. This is done here to not lead to a merge conflict as the button already had this script.

public class ToShip : MonoBehaviourPunCallbacks
{
    // Leave's the photon room and then on the callback goes to the choose ship screen.

    public void goToShip()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Choose Ship");
    }
}
