using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BasicCannonController : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool cannonActive;
    public bool masterCannon;
    public bool invertControls;

    public float rotationSensitivity;
    public float power;
    public float reloadTime;

    public GameObject ammoType;
    public Transform shotOrigin;

    bool reloading;
    float ammoLevel;

    bool changingWeaponSignal;
    bool changedWeapon;
    public int currentWeapon;

    bool serverShootFlag;
    bool sendShootToClient;
    bool clientShootFlag;

    int currentTargetId;
    public bool lockedOn;
    bool localLockOn;
    Vector3 freeFireTargetPos;

    // Start is called before the first frame update
    void Start()
    {
        switchWeapon(0);
        getAmmoLevel();

        serverShootFlag = sendShootToClient = clientShootFlag = changingWeaponSignal = changedWeapon = false;
    }

    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            //Do something?
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, MAYBE NOT USEFUL OUTSIDE OF TUTORIAL?
        DontDestroyOnLoad(this.gameObject);
    }

    void ServerUpdate()
    {
        getInput();

        //fire at target we are locked to
        if (lockedOn)
        {
            currentTargetId = getShipTransform().GetComponent<TargetingSystem>().currentTargetId;
        } //free fire
        else
        {
            freeFireTargetPos = getShipTransform().GetComponent<TargetingSystem>().freeFireTargetPos;
        }

        if (serverShootFlag)
        {
            serverShootFlag = false;
            fire();
        }
    }
    void ClientUpdate()
    {
        if (clientShootFlag)
        {
            clientShootFlag = false;
            fire();
        }
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

        if (changingWeaponSignal)
        {
            weaponSelect();
            changedWeapon = true;
        }

        //updateLineRenderer();

        removeUsedInput();

        getAmmoLevel();
    }

    void removeUsedInput()
    {
        if (changedWeapon)
        {
            changingWeaponSignal = false;
        }
    }

    //inspired by https://www.youtube.com/watch?v=RnEO3MRPr5Y&ab_channel=AdamKonig
    void getInput()
    {
        if (cannonActive)
        {
            //attempt to fire the cannon
            if (SBControls.shoot.IsHeld() && !reloading && !serverShootFlag && !changingWeaponSignal)
            {
                if (ammoLevel > 0)
                {
                    serverShootFlag = sendShootToClient = true;
                }
            }

        }

        if (SBControls.ammo1.IsHeld() && !serverShootFlag)
        {
            changingWeaponSignal = true;
            currentWeapon = 0;
        }

        //change ammo type to explosive cannonball
        if (SBControls.ammo2.IsHeld() && !serverShootFlag)
        {
            changingWeaponSignal = true;
            currentWeapon = 1;
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

        GameObject newCannonBall = Instantiate(ammoType, shotOrigin.position, shotOrigin.rotation);   
        
        if (!photonView.IsMine)
        {
            newCannonBall.layer = 10;
        }

        GameObject ship = getShipTransform().gameObject;

        GameObject target;
        Vector3 targetPos;
        float targetXVels = 0;
        float targetYVels = 0;
        float targetZVels = 0;
        //if we are lockedOn get target obj, velocity, and pos
        if (lockedOn)
        {
            target = PhotonView.Find(currentTargetId).gameObject;
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
        float time = distToTarget / power;

        float Vy = (-0.5f * time * Physics.gravity.y) + yDiff / time;
        float Vx = xDiff / time;
        float Vz = zDiff / time;

        Vx = Vx + targetXVels;
        Vy = Vy + targetYVels;
        Vz = Vz + targetZVels;

        newCannonBall.GetComponent<Rigidbody>().velocity = new Vector3(Vx, Vy, Vz);
        newCannonBall.GetComponent<CannonballController>().owner = getShipTransform().gameObject;

        weaponStatusReloading();

        updateArsenal(currentWeapon);

        Invoke("weaponStatusReady", 2);        
    }

    //get ammo level for current ammo type
    void getAmmoLevel()
    {
        if (ammoType.name == "Cannonball")
        {
            ammoLevel = getShipTransform().GetComponent<ShipArsenal>().cannonballAmmo;
        }
        if (ammoType.name == "ExplosiveCannonball")
        {
            ammoLevel = getShipTransform().GetComponent<ShipArsenal>().explosiveCannonballAmmo;
        }
    }

    //choose weapon type
    void weaponSelect()
    {
        switchWeapon(currentWeapon);
    }

    //switch to given weapon and reload
    void switchWeapon(int weaponId)
    {
        //check not trying to switch to same ammo that is currently selected
        if (ammoType != getShipTransform().GetComponent<ShipArsenal>().equippedWeapons[weaponId])
        {
            GetComponentInParent<PlayerPhotonHub>().UpdateWeapon(weaponId);
            ammoType = getShipTransform().GetComponent<ShipArsenal>().equippedWeapons[weaponId];
            getAmmoLevel();
            if (ammoLevel > 0)
            {
                weaponStatusReloading();
                Invoke("weaponStatusReady", 2);
            }
        }
    }

    //reload finished
    void weaponStatusReady()
    {
        reloading = false;
    }

    void weaponStatusReloading()
    {
        reloading = true;
        getAmmoLevel();
    }

    //update ships ammo levels
    void updateArsenal(int weaponId)
    {
        getShipTransform().GetComponent<ShipArsenal>().reduceAmmo(weaponId);
        getAmmoLevel();
        GetComponentInParent<PlayerPhotonHub>().UpdateAmmo(ammoType.name, ammoLevel);
    }

    #region IPunStuff

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

            stream.SendNext(changedWeapon);
            if (changedWeapon)
            {
                changedWeapon = false;
            }

            stream.SendNext(currentWeapon);
        }
        else
        {
            ClientPhotonStream(stream, info);

            changingWeaponSignal = (bool)stream.ReceiveNext();
            currentWeapon = (int)stream.ReceiveNext();
        }
    }

    #endregion
}

