using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomMenu : MonoBehaviourPunCallbacks
{
   [SerializeField] private Text roomName;

   public void OnClick_CreateRoom()
   {
        if (roomName.text != "" && PhotonNetwork.NickName != "")
        {
            Connect.LaunchWithRoom(roomName.text);
        }
   }

   public override void OnCreatedRoom()
   {
      Debug.Log("Created room successfully.", this);
   }

   public override void OnCreateRoomFailed(short returnCode, string message)
   {
      Debug.Log("Room created failed: " + message, this);
   }
}
