using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ShipArsenal : MonoBehaviourPun, IPunObservable
{
    public float health;
    public float maxHealth;

    public int cannonballAmmo;
    public int maxCannonballAmmo;

    public int explosiveCannonballAmmo;
    public int maxExplosiveCannonballAmmo;

    public int shockwaveAmmo;
    public int maxShockwaveAmmo;

    public int homingAmmo;
    public int maxHomingAmmo;


    //Dictionary containing all weapons by Id and whether they are equipped on the ship or not
    //in future some script on the ship controller would equip certain weapons depending on the ship type?
    //for now we just equip all the weapons on start
    // 0 = regular cannon, 1 = explosive cannons, 2 = gatling gun, 3 = shockwave cannons, 4 = homing projectiles
    public Dictionary<int, bool> weapons = new Dictionary<int, bool> { { 0, false }, { 1, false }, { 2, false }, { 3, false }, { 4, false } };

    private readonly float regenFactorOfMaxHealth = 0.2f;
    private readonly int regenOfCannonballsPerReloadPeriod = 3;
    private readonly int regenOfExplosiveCannonballPerReloadPeriod = 6;
    private readonly int regenOfSpecialAmmoPerReloadPeriod = 3;

    private void Awake()
    {
        if (PlayerChoices.ship == "lightShip")
        {
            EnableWeapon(0);
            EnableWeapon(1);
            EnableWeapon(4);
        }
        else if (PlayerChoices.ship == "mediumShip")
        {
            EnableWeapon(0);
            EnableWeapon(1);
            EnableWeapon(2);
        }
        else if (PlayerChoices.ship == "heavyShip")
        {
            EnableWeapon(0);
            EnableWeapon(1);
            EnableWeapon(3);
        }
        else
        {
            Debug.LogWarning("Invalid ship type");
        }
    }
    private void Start()
    {
        GetComponent<WeaponsController>().equipWeapons();

        Respawn();
    }

    void EnableWeapon(int weaponId)
    {
        weapons[weaponId] = true;
    }

    public void doDamage(float damage)
    {
        DateTime spawnTime = GetComponent<PlayerController>().spawnTime;

        if ((DateTime.Now - spawnTime).TotalSeconds > 1)
        {
            health -= damage;
        }
    }

    public void HitMe(string weaponName)
    {
        switch (weaponName)
        {
            case "gatling":
                photonView.RPC(nameof(Impact1), RpcTarget.All);
                break;

            default:
                Debug.LogError("Invalid weapon name");
                break;
        }
    }

    [PunRPC]
    void Impact1()
    {
        if (health - 0.1f > 0)
        {
            health -= 0.1f;
        }
    }

    public void Respawn()
    {
        cannonballAmmo = maxCannonballAmmo;
        explosiveCannonballAmmo = maxExplosiveCannonballAmmo;
        shockwaveAmmo = maxShockwaveAmmo;
        homingAmmo = maxHomingAmmo;
        health = maxHealth;
    }

    public void Resupply()
    {
        health = Math.Min(health + regenFactorOfMaxHealth * maxHealth, maxHealth);
        cannonballAmmo = Math.Min(cannonballAmmo + regenOfCannonballsPerReloadPeriod, maxCannonballAmmo);
        explosiveCannonballAmmo = Math.Min(explosiveCannonballAmmo + regenOfExplosiveCannonballPerReloadPeriod, maxExplosiveCannonballAmmo);
        shockwaveAmmo = Math.Min(shockwaveAmmo + regenOfSpecialAmmoPerReloadPeriod, maxShockwaveAmmo);
        homingAmmo = Math.Min(homingAmmo + regenOfSpecialAmmoPerReloadPeriod, maxHomingAmmo);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.Serialize(ref health);
    }
}
