using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerStatus : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{

    public string playerName;
    public bool ready;
    public TeamData.Team team;


    // Creates a photon instance of the PlayerStatus prefab with extra data containing the name and team so they can be set on the other clients
    public static GameObject CreateLocal()
    {
         
        object[] data = {PhotonNetwork.NickName, false, PlayerChoices.team};
        GameObject status = PhotonNetwork.Instantiate("PlayerStatus", new Vector3(0,0,0), Quaternion.identity, 0, data);
        var ps = status.GetComponent<PlayerStatus>();
        ps.playerName = PhotonNetwork.NickName;
        ps.ready = false;
        ps.team = PlayerChoices.team;
        return status;
    }

    public void ReadyUp()
    {
        ready = true;
    }


    // Deletes lobby listing when playerstatus is destroyed
    void OnDestroy()
    {
        Lobby.Instance.DestroyListing(playerName);
    }

    //When this class is photon ninstantiated from another client the extra data that is passed through is used here to set the initial variables
    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        playerName = (string)instantiationData[0];
        ready = (bool)instantiationData[1];
        team = (TeamData.Team)instantiationData[2];
        Debug.Log("NEW PLAYER STATUS " + playerName + " " + team.ToString() + " " + ready.ToString());

        Lobby.Instance.OnNewPlayer(this);
    }
}
