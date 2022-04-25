using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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
