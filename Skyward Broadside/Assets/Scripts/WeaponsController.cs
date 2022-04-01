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
        //enable weapon 0 on start
        EnableWeapon(0);
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
        GetInput();

        lockedOn = GetComponent<TargetingSystem>().lockedOn;

        switch (currentWeaponId)
        {
            case 0:
                DisableExplosiveCannons();
                DisableGatlingGun();
                DisableShockwaveCannons();
                DisableHomingCannons();
                EnableCannons();
                break;

            case 1:
                DisableCannons();
                DisableGatlingGun();
                DisableShockwaveCannons();
                DisableHomingCannons();
                EnableExplosiveCannons();
                break;

            case 2:
                DisableCannons();
                DisableExplosiveCannons();
                DisableShockwaveCannons();
                DisableHomingCannons();
                EnableGatlingGun();
                break;

            case 3:
                DisableCannons();
                DisableExplosiveCannons();
                DisableGatlingGun();
                DisableHomingCannons();
                EnableShockwaveCannons();
                break;

            case 4:
                DisableCannons();
                DisableExplosiveCannons();
                DisableGatlingGun();
                DisableShockwaveCannons();
                EnableHomingCannons();
                break;

            default:
                Debug.LogError("Invalid weapon Id");
                break;
        }
        switchedWeapon = false;
    }

    void GetInput()
    {
        if (SBControls.ammo1.IsDown())
        {
            EnableWeapon(equippedWeapons[0]);
            switchedWeapon = true;
        }
        else if (SBControls.ammo2.IsDown())
        {
            EnableWeapon(equippedWeapons[1]);
            switchedWeapon = true;
        }
        else if (SBControls.ammo3.IsDown())
        {
            EnableWeapon(equippedWeapons[2]);
            switchedWeapon = true;
        }
    }

    void EnableWeapon(int weaponId)
    {
        currentWeaponId = weaponId;
    }

    void EnableCannons()
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

                    if (CheckLineOfSight(cannon) && noOfEnabledCannons < ammoCount)
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

                    if (CheckLineOfSight(cannon) && noOfEnabledCannons < ammoCount)
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

    void DisableCannons()
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

    void EnableExplosiveCannons()
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

                    if (CheckLineOfSight(cannon) && noOfEnabledCannons < ammoCount)
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

                    if (CheckLineOfSight(cannon) && noOfEnabledCannons < ammoCount)
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

    void DisableExplosiveCannons()
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

    void EnableGatlingGun()
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

    void DisableGatlingGun()
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

    void EnableShockwaveCannons()
    {
        int ammoCount = GetComponent<ShipArsenal>().shockwaveAmmo;
        int noOfEnabledCannons = 0;

        if (lockedOn)
        {
            foreach (GameObject cannon in cannons)
            {
                if (cannon != null)
                {
                    if (switchedWeapon)
                    {
                        cannon.GetComponent<ShockwaveCannonController>().reload();
                    }

                    if (CheckLineOfSight(cannon) && noOfEnabledCannons < ammoCount)
                    {
                        cannon.GetComponent<ShockwaveCannonController>().weaponEnabled = true;
                        cannon.GetComponent<ShockwaveCannonController>().lockedOn = true;
                        noOfEnabledCannons++;
                    }
                    else
                    {
                        cannon.GetComponent<ShockwaveCannonController>().weaponEnabled = false;
                        cannon.GetComponent<ShockwaveCannonController>().lockedOn = false;
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
                        cannon.GetComponent<ShockwaveCannonController>().reload();
                    }

                    if (CheckLineOfSight(cannon) && noOfEnabledCannons < ammoCount)
                    {
                        cannon.GetComponent<ShockwaveCannonController>().weaponEnabled = true;
                        cannon.GetComponent<ShockwaveCannonController>().lockedOn = false;
                        noOfEnabledCannons++;
                    }
                    else
                    {
                        cannon.GetComponent<ShockwaveCannonController>().weaponEnabled = false;
                        cannon.GetComponent<ShockwaveCannonController>().lockedOn = false;
                    }
                }
                else
                {
                    Debug.LogWarning("Could not find cannon object");
                }
            }
        }
    }

    void DisableShockwaveCannons()
    {
        foreach (GameObject cannon in cannons)
        {
            if (cannon != null)
            {
                cannon.GetComponent<ShockwaveCannonController>().weaponEnabled = false;
                cannon.GetComponent<ShockwaveCannonController>().lockedOn = false;
            }
            else
            {
                Debug.LogWarning("Could not find cannon object");
            }
        }
    }

    void EnableHomingCannons()
    {
        int ammoCount = GetComponent<ShipArsenal>().homingAmmo;
        int noOfEnabledCannons = 0;

        foreach (GameObject cannon in cannons)
        {
            if (cannon != null)
            {
                if (switchedWeapon)
                {
                    cannon.GetComponent<HomingCannonController>().Reload();
                }

                if (noOfEnabledCannons < ammoCount)
                {
                    cannon.GetComponent<HomingCannonController>().weaponEnabled = true;
                    noOfEnabledCannons++;
                }
                else
                {
                    cannon.GetComponent<HomingCannonController>().weaponEnabled = false;
                }
            }
            else
            {
                Debug.LogWarning("Could not find cannon object");
            }
        }
    }

    void DisableHomingCannons()
    {
        foreach (GameObject cannon in cannons)
        {
            if (cannon != null)
            {
                cannon.GetComponent<HomingCannonController>().weaponEnabled = false;
            }
            else
            {
                Debug.LogWarning("Could not find cannon object");
            }
        }
    }

    bool CheckLineOfSight(GameObject cannon)
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