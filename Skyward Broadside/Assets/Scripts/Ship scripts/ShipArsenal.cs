//This script is responsible for managing the ships ammo and health levels

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

    //Enable appropiate staring weapons depending on ship type
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

    //Equip weapons and spawn the player, make sure the low health fire particles are off
    private void Start()
    {
        GetComponent<WeaponsController>().equipWeapons();

        Respawn();

        GetComponent<ShipController>().PutOutFires();
    }

    //Update called once per frame. If we are on 20 or less health, turn on fire particles
    private void Update()
    {
        if (health <= 20f)
        {
            GetComponent<ShipController>().StartFires();
        }
        else if (health > 20f)
        {
            GetComponent<ShipController>().PutOutFires();
        }
    }

    //Enable the given weapon
    void EnableWeapon(int weaponId)
    {
        weapons[weaponId] = true;
    }

    //Does the given amount of damage to the ship
    public void doDamage(float damage)
    {
        DateTime spawnTime = GetComponent<PlayerController>().spawnTime;

        if ((DateTime.Now - spawnTime).TotalSeconds > 1)
        {
            health -= damage;
        }
    }

    //Damage registering for the gatling gun
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

    //RPC call to update health accross the network
    [PunRPC]
    void Impact1()
    {
        if (health - 0.1f > 0)
        {
            health -= 0.1f;
        }
    }

    //Reset ammo and health on respawn
    public void Respawn()
    {
        cannonballAmmo = maxCannonballAmmo;
        explosiveCannonballAmmo = maxExplosiveCannonballAmmo;
        shockwaveAmmo = maxShockwaveAmmo;
        homingAmmo = maxHomingAmmo;
        health = maxHealth;
    }

    //Add ammo and health whilst in resupply zone
    public void Resupply()
    {
        health = Math.Min(health + regenFactorOfMaxHealth * maxHealth, maxHealth);
        cannonballAmmo = Math.Min(cannonballAmmo + regenOfCannonballsPerReloadPeriod, maxCannonballAmmo);
        explosiveCannonballAmmo = Math.Min(explosiveCannonballAmmo + regenOfExplosiveCannonballPerReloadPeriod, maxExplosiveCannonballAmmo);
        shockwaveAmmo = Math.Min(shockwaveAmmo + regenOfSpecialAmmoPerReloadPeriod, maxShockwaveAmmo);
        homingAmmo = Math.Min(homingAmmo + regenOfSpecialAmmoPerReloadPeriod, maxHomingAmmo);
    }

    //Sync health accross the network
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.Serialize(ref health);
    }
}
