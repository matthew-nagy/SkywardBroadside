using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInfoPPH : MonoBehaviourPun, IPunObservable
{
    Transform parentPlayer;

    public float currHealth;
    // Start is called before the first frame update
    void Start()
    {
        parentPlayer = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            currHealth = parentPlayer.GetComponent<PlayerPhotonHub>().currHealth;
            print(currHealth);
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
