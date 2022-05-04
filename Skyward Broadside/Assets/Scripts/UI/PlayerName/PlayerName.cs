using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{
    //[SerializeField] Text nameText;

    public void AcceptName()
    {
        if (PhotonNetwork.NickName != "")
        {
            SceneManager.LoadScene("Rooms");
        }
    }
}
