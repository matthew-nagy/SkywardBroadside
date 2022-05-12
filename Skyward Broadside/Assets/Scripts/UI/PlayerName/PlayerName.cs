using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{
    //[SerializeField] Text nameText;

    // Go to the room scene if the player's name is not null
    public void AcceptName()
    {
        if (PhotonNetwork.NickName != "")
        {
            SceneManager.LoadScene("Rooms");
        }
    }
}
