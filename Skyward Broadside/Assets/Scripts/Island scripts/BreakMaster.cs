using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BreakMaster : MonoBehaviourPunCallbacks, IPunObservable
{
    List<Breakable> children;


    public bool IsInLocatioOf(Transform t)
    {
        return t.position == transform.position;
    }

    public int RegisterNewChild(Breakable child)
    {
        children.Add(child);
        return children.Count - 1;
    }

    public bool IsPhotonMaster()
    {
        return photonView.IsMine;
    }

    // Start is called before the first frame update
    void Start()
    {
        Blackboard.breakMasters.Add(this);
        Debug.Log("Break master created");
        children = new List<Breakable>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    #region Pun Synchronisation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {

        }
        else
        {

        }

    }
    #endregion
}
