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


struct EventQueue
{
    public List<BreakEvent> breakEvents;
    public List<SyncEvent> syncEvents;

    static public EventQueue Make()
    {
        EventQueue newQueue = new EventQueue();
        newQueue.breakEvents = new List<BreakEvent>();
        newQueue.syncEvents = new List<SyncEvent>();
        return newQueue;
    }
}

public class BreakMaster : MonoBehaviourPunCallbacks, IPunObservable
{
    List<Breakable> children;
    EventQueue events;
    
    //Before being properly setup, events given by the photon master are held here
    //the ? is there so microsoft java will let me null it
    EventQueue? preInstaniateEventHolder = null;
    bool init = false;

    CascadeSystem cascader;


    public bool IsInLocatioOf(Transform t)
    {
        return t.position == transform.position;
    }

    public int RegisterNewChild(Breakable child)
    {
        children.Add(child);
        return children.Count - 1;
    }

    //Called by BreakablePhotonInterface once its done
    public void TriggerFinalSetup(CascadeSystem attatchedCascade)
    {
        if (attatchedCascade != null)
        {
            if (IsPhotonMaster())
            {
                cascader = attatchedCascade;
                cascader.Init(children);
            }
            else
            {
                //Won't be needed
                Destroy(cascader);
            }
        }

        if(preInstaniateEventHolder.HasValue)
        {
            foreach(BreakEvent b in preInstaniateEventHolder.Value.breakEvents)
            {
                HandleBreakEvent(b);
            }
            foreach(SyncEvent s in preInstaniateEventHolder.Value.syncEvents)
            {
                HandleSyncEvent(s);
            }

            preInstaniateEventHolder = null;
        }
    }

    public bool IsPhotonMaster()
    {
        return photonView.IsMine;
    }

    public void RegisterSyncEvent(SyncEvent e)
    {
        events.syncEvents.Add(e);
    }
    public void RegisterBreakEvent(BreakEvent e)
    {
        events.breakEvents.Add(e);
    }

    // Start is called before the first frame update
    void Start()
    {
        Blackboard.breakMasters.Add(this);
        Debug.Log("Break master created");
        children = new List<Breakable>();

        events = EventQueue.Make();

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
        //It could be a delete event when it was already deleted
        if (children[e.indexInOwner] == null)
        {
            return;
        }
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

    void SendEventsDownStream(PhotonStream stream)
    {
        stream.SendNext(events.breakEvents.Count);
        foreach (BreakEvent be in events.breakEvents)
        {
            be.PhotonSend(stream);
        }
        events.breakEvents.Clear();

        stream.SendNext(events.syncEvents.Count);
        foreach (SyncEvent se in events.syncEvents)
        {
            se.PhotonSend(stream);
        }
        events.syncEvents.Clear();
    }

    void ReactToEventsFromStream(PhotonStream stream)
    {
        int recievingBreakEvents = (int)stream.ReceiveNext();
        for (int i = 0; i < recievingBreakEvents; i++)
        {
            BreakEvent be = BreakEvent.PhotonRecieve(stream);
            HandleBreakEvent(be);
        }

        int recirecievingSyncEvent = (int)stream.ReceiveNext();
        for (int i = 0; i < recirecievingSyncEvent; i++)
        {
            SyncEvent se = SyncEvent.PhotonRecieve(stream);
            HandleSyncEvent(se);
        }
    }

    void SendEventsToPreInstantiate(PhotonStream stream)
    {
        if(preInstaniateEventHolder == null)
        {
            preInstaniateEventHolder = EventQueue.Make();
        }

        int recievingBreakEvents = (int)stream.ReceiveNext();
        for (int i = 0; i < recievingBreakEvents; i++)
        {
            preInstaniateEventHolder.Value.breakEvents.Add(BreakEvent.PhotonRecieve(stream));
        }

        int recirecievingSyncEvent = (int)stream.ReceiveNext();
        for (int i = 0; i < recirecievingSyncEvent; i++)
        {
            preInstaniateEventHolder.Value.syncEvents.Add(SyncEvent.PhotonRecieve(stream));
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            SendEventsDownStream(stream);
        }
        else
        {
            //Photon will instantiate this object *and* send all streams to it in one go
            //It may do this before this clients BreakablePhotonInterface has properly instantiated it
            //Therefore do a check here, and either play these events back, or send to a BreakEventBuffer
            if(children != null)
            {
                ReactToEventsFromStream(stream);
            }
            else
            {
                SendEventsToPreInstantiate(stream);
            }
        }

    }
    #endregion
}
