using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonShipInit : MonoBehaviour, IPunInstantiateMagicCallback
{

    [SerializeField] GameObject ship;

    // When a ship is instantiated across photon the variables will be set to their type's default value. This callback allows them to be set to their proper values utilising extra data sent from the ships local instance.
    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        string pName = (string)info.photonView.InstantiationData[0];
        TeamData.Team team = (TeamData.Team)info.photonView.InstantiationData[1];
        PlayerController pc = ship.GetComponent<PlayerController>();
        pc.myTeam = team;
        pc.playerName = pName;
        Scoreboard.Instance.OnNewPlayer(pc);
        Debug.Log("PLAYER: " + pc.playerName + ", TEAM: " + pc.myTeam);
    }
}
