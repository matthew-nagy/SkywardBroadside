using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponsController : MonoBehaviour
{
    [SerializeField]
    List<GameObject> cannons;
    [SerializeField]
    GameObject gatlingGun;
    [SerializeField]
    float cannonThresholdAngle;

    public int currentWeaponId;

    bool lockedOn;
    bool switchedWeapon;

    List<int> equippedWeapons = new List<int>();

    private void Start()
    {
        //enable regular cannons on start
        enableWeapon(0);
    }

    //equip any weapons that are marked as true (enabled) by the ship arsenal
    public void equipWeapons()
    {
        Dictionary<int, bool> allWeapons = GetComponent<ShipArsenal>().weapons;
        foreach (int weaponId in allWeapons.Keys)
        {
            if (allWeapons[weaponId])
            {
                equippedWeapons.Add(weaponId);
            }
        }
    }

    private void Update()
    {
        getInput();

        lockedOn = GetComponent<TargetingSystem>().lockedOn;

        switch (currentWeaponId)
        {
            case 0:
                disableExplosiveCannons();
                disableGatlingGun();
                enableCannons();
                break;

            case 1:
                disableCannons();
                disableGatlingGun();
                enableExplosiveCannons();
                break;

            case 2:
                disableCannons();
                disableExplosiveCannons();
                enableGatlingGun();
                break;

            default:
                Debug.LogError("Invalid weapon Id");
                break;
        }
        switchedWeapon = false;
    }

    void getInput()
    {
        if (SBControls.ammo1.IsHeld())
        {
            enableWeapon(equippedWeapons[0]);
            switchedWeapon = true;
            
        }
        else if (SBControls.ammo2.IsHeld())
        {
            enableWeapon(equippedWeapons[1]);
            switchedWeapon = true;
        }
        else if (SBControls.ammo3.IsHeld())
        {
            enableWeapon(equippedWeapons[2]);
            switchedWeapon = true;
        }
    }

    void enableWeapon(int weaponId)
    {
        currentWeaponId = weaponId;
    }

    void enableCannons()
    {
        int ammoCount = GetComponent<ShipArsenal>().cannonballAmmo;
        int noOfEnabledCannons = 0;

        if (lockedOn)
        {
            foreach (GameObject cannon in cannons)
            {
                if (cannon != null)
                {
                    if (switchedWeapon)
                    {
                        cannon.GetComponent<BasicCannonController>().reload();
                    }

                    if (checkLineOfSight(cannon) && noOfEnabledCannons < ammoCount)
                    {
                        cannon.GetComponent<BasicCannonController>().weaponEnabled = true;
                        cannon.GetComponent<BasicCannonController>().lockedOn = true;
                        noOfEnabledCannons++;
                    }
                    else
                    {
                        cannon.GetComponent<BasicCannonController>().weaponEnabled = false;
                        cannon.GetComponent<BasicCannonController>().lockedOn = false;
                    }
                }
                else
                {
                    Debug.LogWarning("Could not find cannon object");
                }
            }
        }
        else
        {
            GetComponent<TargetingSystem>().aquireFreeFireTarget();
            foreach (GameObject cannon in cannons)
            {
                if (cannon != null)
                {
                    if (switchedWeapon)
                    {
                        cannon.GetComponent<BasicCannonController>().reload();
                    }

                    if (checkLineOfSight(cannon) && noOfEnabledCannons < ammoCount)
                    {
                        cannon.GetComponent<BasicCannonController>().weaponEnabled = true;
                        cannon.GetComponent<BasicCannonController>().lockedOn = false;
                        noOfEnabledCannons++;
                    }
                    else
                    {
                        cannon.GetComponent<BasicCannonController>().weaponEnabled = false;
                        cannon.GetComponent<BasicCannonController>().lockedOn = false;
                    }
                }
                else
                {
                    Debug.LogWarning("Could not find cannon object");
                }
            }
        }
    }

    void disableCannons()
    {
        foreach (GameObject cannon in cannons)
        {
            if (cannon != null)
            {
                cannon.GetComponent<BasicCannonController>().weaponEnabled = false;
                cannon.GetComponent<BasicCannonController>().lockedOn = false;
            }
            else
            {
                Debug.LogWarning("Could not find cannon object");
            }
        }
    }

    void enableExplosiveCannons()
    {
        int ammoCount = GetComponent<ShipArsenal>().explosiveCannonballAmmo;
        int noOfEnabledCannons = 0;

        if (lockedOn)
        {
            foreach (GameObject cannon in cannons)
            {
                if (cannon != null)
                {
                    if (switchedWeapon)
                    {
                        cannon.GetComponent<ExplosiveCannonController>().reload();
                    }

                    if (checkLineOfSight(cannon) && noOfEnabledCannons < ammoCount)
                    {
                        cannon.GetComponent<ExplosiveCannonController>().weaponEnabled = true;
                        cannon.GetComponent<ExplosiveCannonController>().lockedOn = true;
                        noOfEnabledCannons++;
                    }
                    else
                    {
                        cannon.GetComponent<ExplosiveCannonController>().weaponEnabled = false;
                        cannon.GetComponent<ExplosiveCannonController>().lockedOn = false;
                    }
                }
                else
                {
                    Debug.LogWarning("Could not find cannon object");
                }
            }
        }
        else
        {
            GetComponent<TargetingSystem>().aquireFreeFireTarget();
            foreach (GameObject cannon in cannons)
            {
                if (cannon != null)
                {
                    if (switchedWeapon)
                    {
                        cannon.GetComponent<ExplosiveCannonController>().reload();
                    }

                    if (checkLineOfSight(cannon) && noOfEnabledCannons < ammoCount)
                    {
                        cannon.GetComponent<ExplosiveCannonController>().weaponEnabled = true;
                        cannon.GetComponent<ExplosiveCannonController>().lockedOn = false;
                        noOfEnabledCannons++;
                    }
                    else
                    {
                        cannon.GetComponent<ExplosiveCannonController>().weaponEnabled = false;
                        cannon.GetComponent<ExplosiveCannonController>().lockedOn = false;
                    }
                }
                else
                {
                    Debug.LogWarning("Could not find cannon object");
                }
            }
        }
    }

    void disableExplosiveCannons()
    {
        foreach (GameObject cannon in cannons)
        {
            if (cannon != null)
            {
                cannon.GetComponent<ExplosiveCannonController>().weaponEnabled = false;
                cannon.GetComponent<ExplosiveCannonController>().lockedOn = false;
            }
            else
            {
                Debug.LogWarning("Could not find cannon object");
            }
        }
    }

    void enableGatlingGun()
    {
        if (gatlingGun != null)
        {
            gatlingGun.GetComponent<GatlingGunController>().weaponEnabled = true;
        }
        else
        {
            Debug.LogWarning("Could not find gatling gun object");
        }
    }

    void disableGatlingGun()
    {
        if (gatlingGun != null)
        {
            gatlingGun.GetComponent<GatlingGunController>().weaponEnabled = false;
        }
        else
        {
            Debug.LogWarning("Could not find gatling gun object");
        }
    }

    bool checkLineOfSight(GameObject cannon)
    {
        if (cannon != null)
        {
            GameObject target;
            Vector3 targetPos;
            Vector3 vecToTarget;
            if (lockedOn)
            {
                target = PhotonView.Find(GetComponent<TargetingSystem>().currentTargetId).gameObject;
                vecToTarget = target.transform.position - cannon.GetComponent<BasicCannonController>().shotOrigin.transform.position;
            }
            else
            {
                targetPos = GetComponent<TargetingSystem>().freeFireTargetPos;
                vecToTarget = targetPos - cannon.GetComponent<BasicCannonController>().shotOrigin.transform.position;
            }

            float angle = Vector3.Angle(cannon.GetComponent<BasicCannonController>().shotOrigin.forward, vecToTarget);
            if (angle > cannonThresholdAngle)
            {
                return false;
            }
            return true;
        }
        else
        {
            Debug.LogWarning("Could not find cannon object");
            return false;
        }
    }
}