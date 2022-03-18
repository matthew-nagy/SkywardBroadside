using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public struct BreakEvent
{
    //Helps the BreakMaster locate which child its refering to
    public int indexInOwner;
    
    //Data needed to call Break on a breakable
    public float force;
    public Vector3 contactPoint;
    public float forceRadius;

    public void PhotonSend(PhotonStream stream)
    {
        stream.SendNext(indexInOwner);
        stream.SendNext(force);
        stream.SendNext(contactPoint);
        stream.SendNext(forceRadius);
    }

    public static BreakEvent PhotonRecieve(PhotonStream stream)
    {
        BreakEvent be = new BreakEvent();
        be.indexInOwner = (int)stream.ReceiveNext();
        be.force = (float)stream.ReceiveNext();
        be.contactPoint = (Vector3)stream.ReceiveNext();
        be.forceRadius = (float)stream.ReceiveNext();
        return be;
    }
}

[System.Serializable]
public struct SyncEvent
{
    //Helps the BreakMaster locate which child its refering to
    public int indexInOwner;

    //Position and velocity of the object at a given point in time
    public Vector3 position;
    public Vector3 velocity;

    //Should this breakable be deleted
    public bool delete;

    public void PhotonSend(PhotonStream stream)
    {
        stream.SendNext(indexInOwner);
        stream.SendNext(position);
        stream.SendNext(velocity);
        stream.SendNext(delete);
    }

    public static SyncEvent PhotonRecieve(PhotonStream stream)
    {
        SyncEvent se = new SyncEvent();
        se.indexInOwner = (int)stream.ReceiveNext();
        se.position = (Vector3)stream.ReceiveNext();
        se.velocity = (Vector3)stream.ReceiveNext();
        se.delete = (bool)stream.ReceiveNext();
        return se;
    }
}

public class BreakMaster : MonoBehaviourPunCallbacks, IPunObservable
{
    List<Breakable> children;
    List<BreakEvent> breakEvents;
    List<SyncEvent> syncEvents;
    bool init = false;


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

    public void RegisterSyncEvent(SyncEvent e)
    {
        syncEvents.Add(e);
    }
    public void RegisterBreakEvent(BreakEvent e)
    {
        breakEvents.Add(e);
    }

    // Start is called before the first frame update
    void Start()
    {
        Blackboard.breakMasters.Add(this);
        Debug.Log("Break master created");
        children = new List<Breakable>();

        breakEvents = new List<BreakEvent>();
        syncEvents = new List<SyncEvent>();

        init = true;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void HandleBreakEvent(BreakEvent e)
    {
        children[e.indexInOwner].PhotonBreakCommand(e.force, e.contactPoint, e.forceRadius);
    }
    void HandleSyncEvent(SyncEvent e)
    {
        if (e.delete)
        {
            Destroy(children[e.indexInOwner].gameObject);
        }
        else
        {
            children[e.indexInOwner].PhotonSync(e);
        }
    }

    #region Pun Synchronisation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(breakEvents.Count);
            foreach(BreakEvent be in breakEvents)
            {
                be.PhotonSend(stream);
            }
            breakEvents.Clear();

            stream.SendNext(syncEvents.Count);
            foreach(SyncEvent se in syncEvents)
            {
                se.PhotonSend(stream);
            }
            syncEvents.Clear();
        }
        else
        {
            int recievingBreakEvents = (int)stream.ReceiveNext();
            for(int i = 0; i < recievingBreakEvents; i++)
            {
                BreakEvent be = BreakEvent.PhotonRecieve(stream);
                HandleBreakEvent(be);
            }

            int recirecievingSyncEvent = (int)stream.ReceiveNext();
            for (int i = 0; i < recievingBreakEvents; i++)
            {
                SyncEvent se = SyncEvent.PhotonRecieve(stream);
                HandleSyncEvent(se);
            }

            if (children != null)
            {
                foreach (Breakable i in children)
                {
                    i.GetComponent<Renderer>().material.color = new Color(1.0f, 0.0f, 0.0f);
                }
            }
        }

    }
    #endregion
}
