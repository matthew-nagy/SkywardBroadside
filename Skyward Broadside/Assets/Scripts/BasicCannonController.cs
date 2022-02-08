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

    bool shootingSignal;
    bool shot;
    bool reloading;
    float ammoLevel;

    bool changingWeaponSignal;
    bool changedWeapon;
    int currentWeapon;

    // Start is called before the first frame update
    void Start()
    {
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
        removeUsedInput();
        if (photonView.IsMine)
        {
            getInput();
        }
        if (shootingSignal) { 
            fire();
        }
        if (changingWeaponSignal)
        {
            changedWeapon = true;
            weaponSelect();
        }
        getAmmoLevel();
    }

    void removeUsedInput()
    {
        if (shot)
        {
            shot = shootingSignal = false;
        }
        if (changedWeapon)
        {
            changedWeapon = changingWeaponSignal = false;
        }
    }

    //inspired by https://www.youtube.com/watch?v=RnEO3MRPr5Y&ab_channel=AdamKonig
    void getInput() 
    {
        if (controllerActive)
        {
            weaponAim();

            //attempt to fire the cannon
            if (Input.GetKeyDown(KeyCode.Mouse0) && !reloading && !shootingSignal)
            {
                if (ammoLevel > 0)
                {
                    shootingSignal = true;
                }
            }
        } else
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

        weaponSelect();
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

        shot = true;

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
            getShipTransform().GetComponent<ShipArsenal>().cannonballAmmo--;
        }
        if (ammoType.name == "ExplosiveCannonball")
        {
            getShipTransform().GetComponent<ShipArsenal>().explosiveCannonballAmmo--;
        }
    }

    #region IPunStuff

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(shootingSignal);
            stream.SendNext(changingWeaponSignal);
            stream.SendNext(currentWeapon);
        }
        else
        {
            this.shootingSignal = (bool)stream.ReceiveNext();
            this.changingWeaponSignal = (bool)stream.ReceiveNext();
            this.currentWeapon = (int)stream.ReceiveNext();
        }
    }

    #endregion
}

