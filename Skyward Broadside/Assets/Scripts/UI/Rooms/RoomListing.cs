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
        //button.OnClick.AddListener(delegate { OnClick_RoomListing();});
    }

    // Truncate a string to a certain length
    private string Truncate(string str, int maxLength)
    {
        return str.Length <= maxLength ? str : str.Substring(0, maxLength);
    }

    //Set the text using the room's roominfo
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        RoomInfo = roomInfo;
        text.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers + " " + Truncate(roomInfo.Name, 11);
    }
    
    // Set room to text of room listing
    public void OnClick_RoomListing()
    {
        Connect.LaunchWithRoom(RoomInfo.Name);
    }
}
