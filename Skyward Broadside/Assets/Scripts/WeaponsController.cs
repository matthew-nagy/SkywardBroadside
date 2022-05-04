using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponsController : MonoBehaviour
{
    [SerializeField]
    GameObject[] cannons;
    [SerializeField]
    GameObject[] missileTurret;
    [SerializeField]
    GameObject[] gatlingGuns;
    [SerializeField]
    float cannonThresholdAngle;
    [SerializeField]
    float reloadTime;
    GameObject reloadCircle;

    public int currentWeaponId;
    public bool reloading;

    bool lockedOn;
    bool switchedWeapon;

    List<int> equippedWeapons = new List<int>();

    private void Start()
    {
        //enable weapon 0 on start
        EnableWeapon(0);
        reloadCircle = GameObject.FindGameObjectWithTag("ReloadIndicator");
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

    public void Reload()
    {
        if (!reloading && GetComponent<PhotonView>().IsMine)
        {
            Invoke(nameof(Reloading), 0.05f);
            Invoke(nameof(Reloaded), reloadTime);
        }
    }

    void Reloading()
    {
        reloading = true;
        reloadCircle.GetComponent<ReloadIndicator>().Reload();
    }

    void Reloaded()
    {
        reloading = false;
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
        if (transform.root.GetChild(0).GetChild(0).name == "mediumShip")
        {
            foreach (GameObject gun in gatlingGuns)
            {
                if (gun != null)
                {
                    gun.GetComponent<GatlingGunController>().weaponEnabled = true;
                }
                else
                {
                    Debug.LogWarning("Could not find gatling gun object");
                }
            }
        }
    }

    void DisableGatlingGun()
    {
        if (transform.root.GetChild(0).GetChild(0).name == "mediumShip")
        {
            foreach (GameObject gun in gatlingGuns)
            {
                if (gun != null)
                {
                    gun.GetComponent<GatlingGunController>().weaponEnabled = false;
                }
                else
                {
                    Debug.LogWarning("Could not find gatling gun object");
                }
            }
        }
    }

    void EnableShockwaveCannons()
    {
        if (transform.root.GetChild(0).GetChild(0).name == "heavyShip")
        {
            int ammoCount = GetComponent<ShipArsenal>().shockwaveAmmo;
            int noOfEnabledCannons = 0;

            if (lockedOn)
            {
                foreach (GameObject cannon in cannons)
                {
                    if (cannon != null)
                    {
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
    }

    void DisableShockwaveCannons()
    {
        if (transform.root.GetChild(0).GetChild(0).name == "heavyShip")
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
    }

    void EnableHomingCannons()
    {
        if (transform.root.GetChild(0).GetChild(0).name == "lightShip")
        {
            int ammoCount = GetComponent<ShipArsenal>().homingAmmo;
            int noOfEnabledCannons = 0;

            if (lockedOn)
            {
                foreach (GameObject cannon in missileTurret)
                {
                    if (cannon != null)
                    {
                        if (noOfEnabledCannons < ammoCount)
                        {
                            cannon.GetComponent<HomingCannonController>().weaponEnabled = true;
                            cannon.GetComponent<HomingCannonController>().lockedOn = true;
                            noOfEnabledCannons++;
                        }
                        else
                        {
                            cannon.GetComponent<HomingCannonController>().weaponEnabled = false;
                            cannon.GetComponent<HomingCannonController>().lockedOn = false;
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
                foreach (GameObject cannon in missileTurret)
                {
                    if (cannon != null)
                    {
                        if (noOfEnabledCannons < ammoCount)
                        {
                            cannon.GetComponent<HomingCannonController>().weaponEnabled = true;
                            cannon.GetComponent<HomingCannonController>().lockedOn = false;
                            noOfEnabledCannons++;
                        }
                        else
                        {
                            cannon.GetComponent<HomingCannonController>().weaponEnabled = false;
                            cannon.GetComponent<HomingCannonController>().lockedOn = false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Could not find cannon object");
                    }
                }
            }
        }
    }

    void DisableHomingCannons()
    {
        if (transform.root.GetChild(0).GetChild(0).name == "lightShip")
        {
            foreach (GameObject cannon in missileTurret)
            {
                if (cannon != null)
                {
                    cannon.GetComponent<HomingCannonController>().weaponEnabled = false;
                    cannon.GetComponent<HomingCannonController>().lockedOn = false;
                }
                else
                {
                    Debug.LogWarning("Could not find cannon object");
                }
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