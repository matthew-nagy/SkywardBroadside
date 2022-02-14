using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BasicCannonController : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool controllerActive;
    public bool masterCannon;

    public float rotationSensitivity;
    public float power;
    public float reloadTime;

    public GameObject ammoType;
    public Transform shotOrigin;

    KeyCode secondaryFireKey = KeyCode.O;


    bool shootingSignal;
    bool shot;
    bool reloading;
    float ammoLevel;

    bool changingWeaponSignal;
    bool changedWeapon;
    public int currentWeapon;

    // Start is called before the first frame update
    void Start()
    {
        switchWeapon(0);
        getAmmoLevel();

        //set the "aiming line" to green to show weapons are ready
        transform.GetComponent<LineRenderer>().startColor = Color.green;
        transform.GetComponent<LineRenderer>().endColor = Color.green;

        shootingSignal = shot = changingWeaponSignal = changedWeapon = false;
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

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            getInput();
        }

        if (shootingSignal)
        {
            fire();
            shot = true;
        }

        if (changingWeaponSignal)
        {
            weaponSelect();
            changedWeapon = true;
        }

        removeUsedInput();

        getAmmoLevel();
    }

    void removeUsedInput()
    {
        if (shootingSignal)
        {
            shootingSignal = false;
        }
        if (changedWeapon)
        {
            changingWeaponSignal = false;
        }
    }

    //inspired by https://www.youtube.com/watch?v=RnEO3MRPr5Y&ab_channel=AdamKonig
    void getInput()
    {
        if (controllerActive)
        {
            weaponAim();

            //attempt to fire the cannon
            if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(secondaryFireKey)) && !reloading && !shootingSignal)
            {
                if (ammoLevel > 0)
                {
                    shootingSignal = true;
                }
            }

        }
        else
        {
            transform.GetComponent<LineRenderer>().enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && !reloading && !shootingSignal)
        {
            changingWeaponSignal = true;
            currentWeapon = 0;
        }

        //change ammo type to explosive cannonball
        if (Input.GetKeyDown(KeyCode.Alpha2) && !reloading && !shootingSignal)
        {
            changingWeaponSignal = true;
            currentWeapon = 1;
        }
    }

    Transform getShipTransform()
    {
        return transform.root.GetChild(0);
    }

    //fire the cannon
    void fire()
    {

        GameObject newCannonBall = Instantiate(ammoType, shotOrigin.position, shotOrigin.rotation);
        newCannonBall.GetComponent<Rigidbody>().velocity = shotOrigin.transform.forward * power;
        newCannonBall.GetComponent<CannonballController>().owner = getShipTransform().gameObject;

        weaponStatusReloading();

        updateArsenal();

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

    //aim weapons
    void weaponAim()
    {
        float rotationInput = Input.GetAxisRaw("Mouse Y");
        transform.Rotate(new Vector3(0, 0, rotationInput));
        transform.GetComponent<LineRenderer>().enabled = true;
    }

    //reload finished and if ammo is available set aiming line to green
    void weaponStatusReady()
    {
        reloading = false;
        getAmmoLevel();
        if (ammoLevel > 0)
        {
            transform.GetComponent<LineRenderer>().startColor = Color.green;
            transform.GetComponent<LineRenderer>().endColor = Color.green;
        }
    }

    //set the "aiming line" to red to show weapons are reloading
    void weaponStatusReloading()
    {
        reloading = true;
        transform.GetComponent<LineRenderer>().startColor = Color.red;
        transform.GetComponent<LineRenderer>().endColor = Color.red;
    }

    //update ships ammo levels
    void updateArsenal()
    {
        if (ammoType.name == "Cannonball")
        {
            float ammoLevel = getShipTransform().GetComponent<ShipArsenal>().cannonballAmmo--;
            GetComponentInParent<PlayerPhotonHub>().UpdateAmmo(ammoType.name, ammoLevel);
            
        }
        if (ammoType.name == "ExplosiveCannonball")
        {
            float ammoLevel = getShipTransform().GetComponent<ShipArsenal>().explosiveCannonballAmmo--;
            GetComponentInParent<PlayerPhotonHub>().UpdateAmmo(ammoType.name, ammoLevel);
        }
    }

    #region IPunStuff

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(shot);
            if (shot)
            {
                stream.SendNext(transform.rotation);
                stream.SendNext(transform.position);
                shot = false;
            }

            stream.SendNext(changedWeapon);
            if (changedWeapon)
            {
                changedWeapon = false;
            }

            stream.SendNext(currentWeapon);
        }
        else
        {
            shootingSignal = (bool)stream.ReceiveNext();
            if (shootingSignal)
            {
                transform.rotation = ((Quaternion)stream.ReceiveNext());
                transform.position = ((Vector3)stream.ReceiveNext());
            }

            changingWeaponSignal = (bool)stream.ReceiveNext();
            currentWeapon = (int)stream.ReceiveNext();
        }
    }

    #endregion
}

