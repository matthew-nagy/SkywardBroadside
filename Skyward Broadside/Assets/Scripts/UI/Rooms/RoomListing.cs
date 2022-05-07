using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomListing : MonoBehaviour
{
    [SerializeField] private Text text;
    
    public RoomInfo RoomInfo { get; private set; }
    
    void Start()
    {
        Button button = gameObject.GetComponent<Button>();
        //button.onClick.AddListener(delegate { OnClick_RoomListing();});
    }

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        RoomInfo = roomInfo;
        text.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers + " " + roomInfo.Name;
    }
    
    public void OnClick_RoomListing()
    {
        Connect.LaunchWithRoom(RoomInfo.Name);
    }
}
