using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCannonController : MonoBehaviour
{
    public bool controllerActive;
    public bool masterCannon;

    public float rotationSensitivity;
    public float power;
    public float reloadTime;

    public GameObject ammoType;
    public Transform shotOrigin;

    bool shooting;
    bool reloading;
    float ammoLevel;

    // Start is called before the first frame update
    void Start()
    {
        getAmmoLevel();

        //set the "aiming line" to green to show weapons are ready
        transform.GetComponent<LineRenderer>().startColor = Color.green;
        transform.GetComponent<LineRenderer>().endColor = Color.green;
    }

    // Update is called once per frame
    void Update()
    {
        getInput();
        getAmmoLevel();
    }

    //inspired by https://www.youtube.com/watch?v=RnEO3MRPr5Y&ab_channel=AdamKonig
    void getInput() 
    {
        if (controllerActive)
        {
            weaponAim();

            //attempt to fire the cannon
            if (Input.GetKeyDown(KeyCode.Mouse0) && !reloading && !shooting)
            {
                if (ammoLevel > 0)
                {
                    fire();
                }
            }
        } else
        {
            transform.GetComponent<LineRenderer>().enabled = false;
        }

        weaponSelect();
    }

    //fire the cannon
    void fire()
    {
        shooting = true;

        GameObject newCannonBall = Instantiate(ammoType, shotOrigin.position, shotOrigin.rotation);
        newCannonBall.GetComponent<Rigidbody>().velocity = shotOrigin.transform.forward * power;

        shooting = false;

        weaponStatusReloading();

        updateArsenal();

        Invoke("weaponStatusReady", 2);        
    }

    //get ammo level for current ammo type
    void getAmmoLevel()
    {
        if (ammoType.name == "Cannonball")
        {
            ammoLevel = transform.root.GetComponent<ShipArsenal>().cannonballAmmo;
        }
        if (ammoType.name == "ExplosiveCannonball")
        {
            ammoLevel = transform.root.GetComponent<ShipArsenal>().explosiveCannonballAmmo;
        }
    }

    //choose weapon type
    void weaponSelect()
    {
        //change ammo type to cannonball
        if (Input.GetKeyDown(KeyCode.Alpha1) && !reloading && !shooting)
        {
            switchWeapon(0);
        }

        //change ammo type to explosive cannonball
        if (Input.GetKeyDown(KeyCode.Alpha2) && !reloading && !shooting)
        {
            switchWeapon(1);
        }
    }

    //switch to given weapon and reload
    void switchWeapon(int weaponId)
    {
        //check not trying to switch to same ammo that is currently selected
        if (ammoType != transform.root.GetComponent<ShipArsenal>().equippedWeapons[weaponId])
        {
            ammoType = transform.root.GetComponent<ShipArsenal>().equippedWeapons[weaponId];
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
            transform.root.GetComponent<ShipArsenal>().cannonballAmmo--;
        }
        if (ammoType.name == "ExplosiveCannonball")
        {
            transform.root.GetComponent<ShipArsenal>().explosiveCannonballAmmo--;
        }
    }
}

