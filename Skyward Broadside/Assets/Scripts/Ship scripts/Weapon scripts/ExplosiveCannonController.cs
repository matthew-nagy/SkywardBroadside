//This script controls the firing mechanism for regular cannon balls 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ExplosiveCannonController : MonoBehaviourPunCallbacks, IPunObservable
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

    KeyCode secondaryFireKey = KeyCode.Space;

    string shipType;

    [SerializeField]
    ParticleSystem cannonFire;

    [SerializeField]
    GameObject cannonsShots;

    [SerializeField]
    GameObject cantShootFx;

    public bool outOfAmmo;

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

    //update for my photon view
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
            GetShipTransform().GetComponent<ShipArsenal>().explosiveCannonballAmmo--;
            GetShipTransform().GetComponent<WeaponsController>().Reload();
        }
    }

    //update for other photon views
    void ClientUpdate()
    {
        if (clientShootFlag)
        {
            clientShootFlag = false;
            Fire();
            GetShipTransform().GetComponent<ShipArsenal>().explosiveCannonballAmmo--;
            GetShipTransform().GetComponent<WeaponsController>().Reload();
        }
    }

    //get input from player to fire cannon
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
        else if (outOfAmmo)
        {
            if (SBControls.shoot.IsDown())
            {
                transform.root.Find("SoundFxHub").GetComponent<SoundFxHub>().DoEffect(cantShootFx, transform.position);
            }
        }
    }

    //get the transform for of the ship object
    Transform GetShipTransform()
    {
        return transform.root.Find("Ship").Find(shipType);
    }

    //shake the camera
    void SendShakeEvent()
    {
        if (photonView.IsMine)
        {
            ShipController myController = gameObject.GetComponentInParent<ShipController>();
            myController.InformOfFire();
        }
    }

    //do cannon fire particles
    void CreateParticles()
    {
        Instantiate(cannonFire, shotOrigin.position, shotOrigin.rotation);
    }

    //do cannon fire sound effect
    void DoSoundEffect()
    {
        int random = (int)Random.Range(0f, 3f);
        cannonsShots.transform.GetChild(random).GetComponent<AudioSource>().Play();
    }

    //fire the cannon. Make a projectile and give it a velocity towards the target
    void Fire()
    {
        CreateParticles();
        SendShakeEvent();

        GameObject newProjectile = Instantiate(projectile, shotOrigin.position, shotOrigin.rotation);

        DoSoundEffect();

        if (!photonView.IsMine)
        {
            newProjectile.layer = 10;
        }

        GameObject ship = GetShipTransform().gameObject;

        GameObject target;
        Vector3 targetPos;
        float targetXVels = 0;
        float targetYVels = 0;
        float targetZVels = 0;
        //if we are lockedOn get target obj, velocity, and pos
        if (lockedOn)
        {
            target = PhotonView.Find(currentTargetId).gameObject;
            newProjectile.GetComponent<CannonballController>().target = target;
            targetPos = PhotonView.Find(currentTargetId).transform.position;
            targetXVels = target.GetComponent<Rigidbody>().velocity.x;
            targetYVels = target.GetComponent<Rigidbody>().velocity.y;
            targetZVels = target.GetComponent<Rigidbody>().velocity.z;
        } //if we are free firing, just get target pos
        else
        {
            targetPos = freeFireTargetPos;
        }

        float xDiff = targetPos.x - ship.transform.position.x;
        float yDiff = targetPos.y - ship.transform.position.y;
        float zDiff = targetPos.z - ship.transform.position.z;

        float distToTarget = Mathf.Sqrt(xDiff * xDiff + zDiff * zDiff);
        float time = distToTarget / shotPower;

        float Vy = (-0.5f * time * Physics.gravity.y) + yDiff / time;
        float Vx = xDiff / time;
        float Vz = zDiff / time;

        Vx = Vx + targetXVels;
        Vy = Vy + targetYVels;
        Vz = Vz + targetZVels;

        newProjectile.GetComponent<Rigidbody>().velocity = new Vector3(Vx, Vy, Vz);
        newProjectile.GetComponent<CannonballController>().owner = GetShipTransform().gameObject;
        newProjectile.GetComponent<Explosive>().owner = GetShipTransform().gameObject;
    }

    //sync variables over network
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
