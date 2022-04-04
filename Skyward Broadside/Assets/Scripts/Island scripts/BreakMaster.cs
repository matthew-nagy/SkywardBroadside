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

    static int VertexLimit = 65535;

    List<GameObject> renderers;

    CascadeSystem cascader;


    GameObject MakeRenderer(List<CombineInstance> combine)
    {
        GameObject newRenderer = new GameObject();
        MeshFilter mf = newRenderer.AddComponent<MeshFilter>();
        MeshRenderer mr = newRenderer.AddComponent<MeshRenderer>();

        mf.mesh.CombineMeshes(combine.ToArray());
        mr.material = children[0].GetComponent<Renderer>().material;
        mr.enabled = true;

        return newRenderer;
    }

    void SetupPrimeRenderer()
    {
        renderers = new List<GameObject>();
        List<CombineInstance> combines = new List<CombineInstance>();
        int currentVertNumber = 0;

        foreach(Breakable b in children)
        {
            b.GetComponent<Renderer>().enabled = false;
            Mesh bMesh = b.GetComponent<MeshFilter>().sharedMesh;

            if((currentVertNumber + bMesh.vertexCount) > VertexLimit)
            {
                renderers.Add(MakeRenderer(combines));
                combines.Clear();
                currentVertNumber = 0;
            }

            CombineInstance newInstance = new CombineInstance();
            newInstance.mesh = bMesh;
            newInstance.transform = b.transform.localToWorldMatrix;
            combines.Add(newInstance);
            currentVertNumber += bMesh.vertexCount;
        }

        renderers.Add(MakeRenderer(combines));
    }

    /*void SetupPrimeRendererPerhaps()
    {
        primeRenderer = new GameObject();
        CombineInstance[] combine = new CombineInstance[children.Count];

        int i = 0;
        foreach (Breakable b in children)
        {
            b.GetComponent<Renderer>().enabled = false;
            combine[i].mesh = b.GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = b.transform.localToWorldMatrix;
            i += 1;
        }

        MeshFilter mf = primeRenderer.AddComponent<MeshFilter>();
        MeshRenderer mr = primeRenderer.AddComponent<MeshRenderer>();

        mf.mesh.CombineMeshes(combine);
        mr.material = children[0].GetComponent<Renderer>().material;
        mr.enabled = true;

    }

    GameObject GetRenderer(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv, List<int> tris)
    {
        GameObject renderer = new GameObject();

        //Turn the lists into the arrays needed by a mesh
        Vector3[] finalVertices = new Vector3[vertices.Count];
        Vector3[] finalNormals = new Vector3[normals.Count];
        Vector2[] finalUV = new Vector2[uv.Count];
        int[] finalTris = new int[tris.Count];

        vertices.CopyTo(finalVertices);
        normals.CopyTo(finalNormals);
        uv.CopyTo(finalUV);
        tris.CopyTo(finalTris);

        //Setup the new mesh
        Mesh myMesh = new Mesh();
        myMesh.vertices = finalVertices;
        myMesh.triangles = finalTris;
        myMesh.uv = finalUV;
        myMesh.normals = finalNormals;

        //Assign the components needed for rendering
        MeshRenderer mr = renderer.AddComponent<MeshRenderer>();
        MeshFilter mf = renderer.AddComponent<MeshFilter>();

        //Final setup and return
        mf.sharedMesh = myMesh;
        mr.material = children[0].GetComponent<Renderer>().material;
        mr.enabled = true;

        Debug.LogWarning("Made a mesh for " + name);

        return renderer;
    }

    void SetupPrimeRenderer()
    {
        renderers = new List<GameObject>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        Debug.LogWarning(name + " has " + children.Count + "children");
        foreach(Breakable b in children)
        {
            b.GetComponent<Renderer>().enabled = false;
            Mesh bMesh = b.GetComponent<MeshFilter>().sharedMesh;
            int additionIndex;

            //If we have hit a size limit, create a renderer with what we have so far, and get started on a new mesh
            if((vertices.Count + bMesh.vertices.Length) > VertexLimit)
            {
                renderers.Add(GetRenderer(vertices, normals, uvs, tris));
                vertices.Clear();
                normals.Clear();
                uvs.Clear();
                tris.Clear();
                additionIndex = 0;
            }
            else
            {
                additionIndex = vertices.Count;
            }

            var transformMat = b.transform.localToWorldMatrix;
            Quaternion rotation = b.transform.rotation;
            for(int i = 0; i < bMesh.vertexCount; i++)
            {
                vertices.Add(transformMat.MultiplyPoint3x4(bMesh.vertices[i]));
                normals.Add(rotation * bMesh.normals[i]);
            }
            uvs.AddRange(bMesh.uv);
            for (int i = 0; i < bMesh.triangles.Length; i++)
            {
                tris.Add(bMesh.triangles[i] + additionIndex);
            }

        }

        renderers.Add(GetRenderer(vertices, normals, uvs, tris));

        Debug.LogWarning("Got the mesh for object '" + name + "'");
    }*/

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

        SetupPrimeRenderer();
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
        cascader.InformOfBreak(children[e.indexInOwner]);
    }

    // Start is called before the first frame update
    void Start()
    {
        Blackboard.breakMasters.Add(this);
        //Debug.Log("Break master created");
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
