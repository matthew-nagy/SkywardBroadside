using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GatlingGunController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    ParticleSystem bulletTracer;
    [SerializeField]
    Transform shotOrigin;
    [SerializeField]
    LayerMask myLayerMask;
    [SerializeField]
    LayerMask otherLayerMask;
    [SerializeField]
    float range;
    [SerializeField]
    ParticleSystem gatlingImpact;

    LayerMask layerMask;

    public bool weaponEnabled;

    bool reloading;
    bool serverShootingFlag;
    bool sendShootingToClient;
    bool clientShootingFlag;

    string shooterName;

    Vector3 targetPos;

    KeyCode secondaryFireKey = KeyCode.Space;

    float shotTime;

    string shipType;

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
        shipType = transform.root.GetComponent<PlayerPhotonHub>().shipType;
        shooterName = transform.root.GetComponent<PlayerController>().playerName;
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
            Fire();
        }
    }

    void ClientUpdate()
    {
        if (clientShootingFlag)
        {
            Fire();
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
        else
        {
            serverShootingFlag = sendShootingToClient = false;
        }
    }
    Transform getShipTransform()
    {
        return transform.root.Find("Ship").Find(shipType);
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
    void Fire()
    {
        SendShakeEvent();

        RaycastHit hit;
        Vector3 dir = (targetPos - shotOrigin.transform.position).normalized;
        if (Physics.Raycast(shotOrigin.transform.position, dir, out hit, range, layerMask))
        {
            if (hit.collider.gameObject.name == shipType)
            {
                hit.collider.gameObject.GetComponent<ShipArsenal>().HitMe("gatling");
                hit.collider.gameObject.GetComponent<PlayerController>().lastHit(shooterName);
            }

            ParticleSystem tracer = Instantiate(bulletTracer, shotOrigin.transform.position, Quaternion.LookRotation(dir));

            StartCoroutine(SpawnTracer(tracer, hit.point));

            shotTime = Time.time;
        }
        else
        {   
            hit.point = shotOrigin.transform.position + (dir * range);
            ParticleSystem tracer = Instantiate(bulletTracer, shotOrigin.transform.position, Quaternion.LookRotation(dir));
            StartCoroutine(SpawnTracer(tracer, hit.point));
        }
    }

    IEnumerator SpawnTracer(ParticleSystem tracer, Vector3 hitPoint)
    {
        float time = 0;
        while (time < 0.5)
        {
            Instantiate(gatlingImpact, hitPoint, Quaternion.FromToRotation(hitPoint, transform.position));
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(tracer, tracer.time);
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
