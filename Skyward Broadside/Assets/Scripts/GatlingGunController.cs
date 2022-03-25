using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GatlingGunController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    TrailRenderer bulletTracer;
    [SerializeField]
    Transform shotOrigin;
    [SerializeField]
    LayerMask myLayerMask;
    [SerializeField]
    LayerMask otherLayerMask;
    [SerializeField]
    float range;

    LayerMask layerMask;

    public bool weaponEnabled;

    bool reloading;
    bool serverShootingFlag;
    bool sendShootingToClient;
    bool clientShootingFlag;

    Vector3 targetPos;

    KeyCode secondaryFireKey = KeyCode.Space;

    float shotTime;

    void Awake()
    {
        // we flag as don't destroy on load so that instance survives level synchronization, MAYBE NOT USEFUL OUTSIDE OF TUTORIAL?
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        serverShootingFlag = sendShootingToClient = clientShootingFlag = false;
        SetLayerMask();
    }

    void SetLayerMask()
    {
        if (photonView.IsMine)
        {
            layerMask = myLayerMask;
        }
        else
        {
            layerMask = otherLayerMask;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            ServerUpdate();
        }
        else
        {
            ClientUpdate();
        }
    }

    void ServerUpdate()
    {
        getInput();

        if (serverShootingFlag)
        {
            getShipTransform().GetComponent<TargetingSystem>().aquireFreeFireTarget();
            targetPos = getShipTransform().GetComponent<TargetingSystem>().freeFireTargetPos;
            fire();
        }
    }

    void ClientUpdate()
    {
        if (clientShootingFlag)
        {
            fire();
        }
    }

    void getInput()
    {
        if (weaponEnabled)
        {
            //attempt to fire the cannon
            if ((Input.GetKey(KeyCode.Mouse0) || Input.GetKey(secondaryFireKey)) && !reloading && !serverShootingFlag)
            {
                serverShootingFlag = sendShootingToClient = true;
            }
            else
            {
                serverShootingFlag = sendShootingToClient = false;
            }
        }
    }
    Transform getShipTransform()
    {
        return transform.root.GetChild(0);
    }

    void SendShakeEvent()
    {
        if (photonView.IsMine)
        {
            ShipController myController = gameObject.GetComponentInParent<ShipController>();
            myController.InformOfFire();
        }
    }

    //fire the cannon
    void fire()
    {
        SendShakeEvent();

        RaycastHit hit;
        Vector3 dir = (targetPos - shotOrigin.transform.position).normalized;
        if (Physics.Raycast(shotOrigin.transform.position, dir, out hit, range, layerMask))
        {
            if (hit.collider.gameObject.name == "Ship")
            {
                hit.collider.gameObject.GetComponent<ShipArsenal>().HitMe("gatling");
            }

            TrailRenderer tracer = Instantiate(bulletTracer, shotOrigin.transform.position, Quaternion.identity);

            StartCoroutine(SpawnTracer(tracer, hit));

            shotTime = Time.time;
        }
        else
        {
            TrailRenderer tracer = Instantiate(bulletTracer, shotOrigin.transform.position, Quaternion.identity);
            hit.point = shotOrigin.transform.position + (dir * range);
            StartCoroutine(SpawnTracer(tracer, hit));
        }
    }

    IEnumerator SpawnTracer(TrailRenderer tracer, RaycastHit hit)
    {
        float time = 0;
        Vector3 startPos = tracer.transform.position;

        while (time < 1)
        {
            tracer.transform.position = Vector3.Lerp(startPos, hit.point, time);
            time += Time.deltaTime / tracer.time;

            yield return null;
        }
        tracer.transform.position = hit.point;
        Destroy(tracer.gameObject, tracer.time);
    }

    public void reload()
    {
        reloading = true;
        Invoke(nameof(weaponStatusReady), 2);
    }

    void weaponStatusReady()
    {
        reloading = false;
    }

    void ServerPhotonStream(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.SendNext(sendShootingToClient);
        if (sendShootingToClient)
        {
            sendShootingToClient = false;

            stream.SendNext(transform.rotation);
        }
        stream.SendNext(targetPos);
    }

    void ClientPhotonStream(PhotonStream stream, PhotonMessageInfo info)
    {
        clientShootingFlag = (bool)stream.ReceiveNext();
        if (clientShootingFlag)
        {
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
        targetPos = (Vector3)stream.ReceiveNext();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            ServerPhotonStream(stream, info);
        }
        else
        {
            ClientPhotonStream(stream, info);
        }
    }
}
