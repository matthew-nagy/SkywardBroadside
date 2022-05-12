//This script controls the firing of teh gatling gun.
//Uses raycast to find the hit point and apply damage if an enemy ship is hit

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
    [SerializeField]
    GameObject gatlingBurst;

    GameObject gatlingSoundObj;

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
        shipType = transform.root.Find("Ship").GetChild(0).transform.name;
        shooterName = transform.root.Find("Ship").GetChild(0).GetComponent<PlayerController>().playerName;
        gatlingSoundObj = Instantiate(gatlingBurst, transform.position, transform.rotation);
        gatlingSoundObj.transform.parent = transform.root.GetChild(0).GetChild(0);
    }

    //Sssign a layer mask depending on if the photon view is ours
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

    //Update for when photon view is ours
    void ServerUpdate()
    {
        getInput();

        if (serverShootingFlag)
        {
            getShipTransform().GetComponent<TargetingSystem>().aquireFreeFireTarget();
            targetPos = getShipTransform().GetComponent<TargetingSystem>().freeFireTargetPos;
            Fire();
            PlaySound();
        }
        else
        {
            StopSound();
        }
    }

    //Update for when photon view is not ours
    void ClientUpdate()
    {
        if (clientShootingFlag)
        {
            Fire();
            PlaySound();
        }
        else
        {
            StopSound();
        }
    }

    //Get the player input to fire the gatling gun
    void getInput()
    {
        if (weaponEnabled)
        {
            //attempt to fire the cannon
            if ((Input.GetKey(KeyCode.Mouse0) || Input.GetKey(secondaryFireKey)) && !reloading)
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

    //Play the fire sound whilst firing
    void PlaySound()
    {
        if (!gatlingSoundObj.GetComponent<AudioSource>().isPlaying)
        {
            gatlingSoundObj.GetComponent<AudioSource>().Play();
        }
    }

    //Stop playing the fire sound when not firing
    void StopSound()
    {
        if (gatlingSoundObj.GetComponent<AudioSource>().isPlaying)
        {
            gatlingSoundObj.GetComponent<AudioSource>().Stop();
        }
    }

    //Get the transform of the ship obj
    Transform getShipTransform()
    {
        return transform.root.Find("Ship").Find(shipType);
    }

    //No camera shake for gatling gun
    void SendShakeEvent()
    {
    }

    //fire the gatling gun whilst player input is registered
    void Fire()
    {
        SendShakeEvent();

        RaycastHit hit;
        Vector3 dir = (targetPos - shotOrigin.transform.position).normalized;
        if (Physics.Raycast(shotOrigin.transform.position, dir, out hit, range, layerMask))
        {
            if (hit.collider.gameObject.name.Contains("Ship"))
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

    //A Coroutine for continuously creating a tracer particle effect in the direction of fire
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

    //Send data accross teh network about firing the gatling gun
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
