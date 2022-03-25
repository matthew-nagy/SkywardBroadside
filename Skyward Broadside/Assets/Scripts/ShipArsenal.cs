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
    

    //Dictionary containing all weapons by Id and whether they are equipped on the ship or not
    //in future some script on the ship controller would equip certain weapons depending on the ship type?
    //for now we just equip all the weapons on start
    // 0 = regular cannon, 1 = explosive cannons, 2 = gatling gun, 3 = shockwave cannons
    public Dictionary<int, bool> weapons = new Dictionary<int, bool> { { 0, false }, { 1, false }, { 2, false }, { 3, false } };

    private readonly float regenFactorOfMaxHealth = 0.05f;
    private readonly int regenOfCannonballsPerReloadPeriod = 3;
    private readonly int regenOfExplosiveCannonballPerReloadPeriod = 1;

    private void Start()
    {
        enableWeapon(0);
        enableWeapon(3);
        enableWeapon(2);
        GetComponent<WeaponsController>().equipWeapons();

        cannonballAmmo = maxCannonballAmmo;
        explosiveCannonballAmmo = maxExplosiveCannonballAmmo;
        shockwaveAmmo = maxShockwaveAmmo;
        health = maxHealth;
    }

    void enableWeapon(int weaponId)
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
        health -= 1f;
    }

    public void Respawn()
    {
        cannonballAmmo = maxCannonballAmmo;
        explosiveCannonballAmmo = maxExplosiveCannonballAmmo;
        shockwaveAmmo = maxShockwaveAmmo;
        health = maxHealth;
    }

    public void Resupply()
    {
        health = Math.Min(health + regenFactorOfMaxHealth * maxHealth, maxHealth);
        cannonballAmmo = Math.Min(cannonballAmmo + regenOfCannonballsPerReloadPeriod, maxCannonballAmmo);
        explosiveCannonballAmmo = Math.Min(explosiveCannonballAmmo + regenOfExplosiveCannonballPerReloadPeriod, maxExplosiveCannonballAmmo);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.Serialize(ref health);
    }
}
