using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HomingCannonController : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool weaponEnabled;
    public float shotPower;
    public GameObject projectile;
    public Transform shotOrigin;

    bool reloading;
    bool serverShootFlag;
    bool sendShootToClient;
    bool clientShootFlag;

    int currentTargetId;
    public bool lockedOn;
    bool localLockOn;
    Vector3 freeFireTargetPos;

    string shipType;

    [SerializeField]
    ParticleSystem cannonFire;

    void Awake()
    {
        // we flag as don't destroy on load so that instance survives level synchronization, MAYBE NOT USEFUL OUTSIDE OF TUTORIAL?
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        serverShootFlag = sendShootToClient = clientShootFlag = false;
        shipType = transform.root.Find("Ship").GetChild(0).transform.name;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            ServerUpdate();
            localLockOn = lockedOn;
        }
        else
        {
            lockedOn = localLockOn;
            ClientUpdate();
        }
    }

    void ServerUpdate()
    {
        reloading = GetShipTransform().GetComponent<WeaponsController>().reloading;
        GetInput();

        //set current target Id if we are lockedOn
        if (lockedOn)
        {
            currentTargetId = GetShipTransform().GetComponent<TargetingSystem>().currentTargetId;
        } //or free fire
        else
        {
            freeFireTargetPos = GetShipTransform().GetComponent<TargetingSystem>().freeFireTargetPos;
        }

        if (serverShootFlag)
        {
            serverShootFlag = false;
            Fire();
            GetShipTransform().GetComponent<ShipArsenal>().homingAmmo--;
            GetShipTransform().GetComponent<WeaponsController>().Reload();
        }
    }

    void ClientUpdate()
    {
        if (clientShootFlag)
        {
            clientShootFlag = false;
            Fire();
            GetShipTransform().GetComponent<ShipArsenal>().homingAmmo--;
            GetShipTransform().GetComponent<WeaponsController>().Reload();
        }
    }

    void GetInput()
    {
        if (weaponEnabled)
        {
            //attempt to fire the cannon
            if (SBControls.shoot.IsDown() && !reloading && !serverShootFlag)
            {
                serverShootFlag = sendShootToClient = true;
            }
        }
    }
    Transform GetShipTransform()
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

    void CreateParticles()
    {
        Instantiate(cannonFire, shotOrigin.position, shotOrigin.rotation);
    }

    //fire the cannon
    void Fire()
    {
        SendShakeEvent();

        CreateParticles();

        GameObject ship = GetShipTransform().gameObject;
        GameObject newProjectile = Instantiate(projectile, shotOrigin.position, shotOrigin.rotation);

        if (!photonView.IsMine)
        {
            newProjectile.layer = 10;
        }

        //if we are lockedOn get target obj, velocity, and pos
        if (lockedOn)
        {
            GameObject target = PhotonView.Find(currentTargetId).gameObject;
            newProjectile.GetComponent<Missile>().InitialiseMissile(target.transform);

        } //if we are free firing, just get target pos
        else
        {
            GameObject staticTarget = new GameObject();
            staticTarget.transform.position = freeFireTargetPos;
            newProjectile.GetComponent<Missile>().InitialiseMissile(staticTarget.transform);
        }

        newProjectile.GetComponent<Missile>().owner = GetShipTransform().gameObject;
    }

    void ServerPhotonStream(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.SendNext(sendShootToClient);
        if (sendShootToClient)
        {
            sendShootToClient = false;

            stream.SendNext(transform.rotation);
        }
        stream.SendNext(currentTargetId);
        stream.SendNext(freeFireTargetPos);
        stream.SendNext(localLockOn);
    }

    void ClientPhotonStream(PhotonStream stream, PhotonMessageInfo info)
    {
        clientShootFlag = (bool)stream.ReceiveNext();
        if (clientShootFlag)
        {
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
        currentTargetId = (int)stream.ReceiveNext();
        freeFireTargetPos = (Vector3)stream.ReceiveNext();
        localLockOn = (bool)stream.ReceiveNext();
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
