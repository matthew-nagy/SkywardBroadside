using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCannonController : MonoBehaviour
{
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
        print(ammoLevel);
    }

    //inspired by https://www.youtube.com/watch?v=RnEO3MRPr5Y&ab_channel=AdamKonig
    void getInput() 
    {
        weaponAim();

        //attempt to fire the cannon
        if (Input.GetKeyDown(KeyCode.Space) && !reloading && !shooting)
        {
            if (ammoLevel > 0)
            {
                fire();
                reloading = true;
                Invoke("reload", reloadTime);
            }
        }

        weaponSelect();
    }

    //fire the cannon
    void fire()
    {
        shooting = true;

        GameObject newCannonBall = Instantiate(ammoType, shotOrigin.position, shotOrigin.rotation);
        newCannonBall.GetComponent<Rigidbody>().velocity = shotOrigin.transform.forward * power;

        weaponStatusReloading();

        updateArsenal();

        shooting = false;

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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            switchWeapon(0);
        }

        //change ammo type to explosive cannonball
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            switchWeapon(1);
        }
    }

    //switch to given weapon and reload
    void switchWeapon(int weaponId)
    {
        ammoType = transform.root.GetComponent<ShipArsenal>().equippedWeapons[weaponId];
        getAmmoLevel();
        if (ammoLevel > 0)
        {
            weaponStatusReloading();
            Invoke("weaponStatusReady", 2);
        }
    }

    //aim weapons
    void weaponAim()
    {
        float rotationInput = Input.GetAxisRaw("Mouse Y");
        transform.Rotate(new Vector3(0, 0, rotationInput));
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

