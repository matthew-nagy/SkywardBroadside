using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//Class that was made to store some essential information about a player. This class is used the PlayerUI class to update the healthbar and nametag for each player. After refactoring,
//this class now only stores a copy of the player's current health.
public class PlayerInfoPPH : MonoBehaviourPun, IPunObservable
{
    public float currHealth;

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            currHealth = GetComponent<ShipArsenal>().health;
        }
        
    }

    //Synchronise the player's health across the network.
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currHealth);
        }
        else
        {
            currHealth = (float)stream.ReceiveNext();
        }
    }
}
