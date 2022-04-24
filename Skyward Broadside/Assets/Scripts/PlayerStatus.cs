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

    // Start is called before the first frame update
    void Start()
    {
        //if (photonView.IsMine)
        //{
         //   ready = false;
          //  team = PlayerChoices.team;
        //}

        //playerName = GetComponent<PhotonView>().Owner.NickName;
        //Debug.Log("NEW PLAYER STATUS " + playerName + " " + team.ToString() + " " + ready.ToString());
        //Lobby.Instance.OnNewPlayer(this);
    }

    public void ReadyUp()
    {
        ready = true;
    }

    /*public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            Debug.Log("WRITING PHOTON STREAM");
            System.Object[] stats = { playerName, ready, team };
            stream.SendNext(stats);
        }
        else
        {
            Debug.Log("RECEIVED UPDATE TO PLAYERSTATUS");
            bool temp = ready;
            System.Object[] stats = (System.Object[])stream.ReceiveNext();
            playerName = (string)stats[0];
            ready = (bool)stats[1];
            team = (TeamData.Team)stats[2];

            if (ready != temp)
            {
                Lobby.Instance.RefreshListing(this);
            }
        }
    }*/

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("ONPHOTONINSTANTIATE");
        object[] instantiationData = info.photonView.InstantiationData;
        playerName = (string)instantiationData[0];
        ready = (bool)instantiationData[1];
        team = (TeamData.Team)instantiationData[2];
        Debug.Log("NEW PLAYER STATUS " + playerName + " " + team.ToString() + " " + ready.ToString());

        Lobby.Instance.OnNewPlayer(this);
    }
}
