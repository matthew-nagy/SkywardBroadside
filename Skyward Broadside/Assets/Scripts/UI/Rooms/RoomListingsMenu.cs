using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomListingsMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] private RoomListing roomListing;
    [SerializeField] private Transform content;

    private Dictionary<string, RoomListing> listings = new Dictionary<string, RoomListing>();

    // Updates the cached listings when getting updates from photon
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("ROOM LIST UPDATING");
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                if (listings.ContainsKey(info.Name))
                {
                    Destroy(listings[info.Name].gameObject);
                    listings.Remove(info.Name);
                }
            }
            else
            {
                if (listings.ContainsKey(info.Name))
                {
                    RoomListing listing = listings[info.Name];
                    listing.SetRoomInfo(info);
                }
                else
                {
                    RoomListing listing = Instantiate(roomListing, content);
                    listing.SetRoomInfo(info);
                    listings.Add(info.Name, listing);
                }
            }
        }
    }

    // Destroy listings when going to new scene
    void Start()
    {
        SceneManager.sceneLoaded += (arg0, mode) =>
        {
            if (arg0.name != "Rooms")
                return;
            
            DestroyListings();
        };
    }

    // Destroy all listings
    void DestroyListings()
    {
        foreach (RoomListing listing in listings.Values)
        {
            Destroy(listing.gameObject);
        }

        listings.Clear();
    }
}
