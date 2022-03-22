using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipArsenal : MonoBehaviour
{
    public float maxHealth;
    public int maxCannonballAmmo;
    public int maxExplosiveCannonballAmmo;

    public int cannonballAmmo;
    public int explosiveCannonballAmmo;
    public float health;

    //Dictionary containing all weapons by Id and whether they are equipped on the ship or not
    //in future some script on the ship controller would equip certain weapons depending on the ship type?
    //for now we just equip all the weapons on start
    // 0 = regular cannon, 1 = explosive cannons
    public Dictionary<int, bool> weapons = new Dictionary<int, bool> { { 0, false }, { 1, false }, { 2, false} };

    private readonly float regenFactorOfMaxHealth = 0.05f;
    private readonly int regenOfCannonballsPerReloadPeriod = 3;
    private readonly int regenOfExplosiveCannonballPerReloadPeriod = 1;

    private void Start()
    {
        enableWeapon(0);
        enableWeapon(1);
        enableWeapon(2);
        GetComponent<WeaponsController>().equipWeapons();

        cannonballAmmo = maxCannonballAmmo;
        explosiveCannonballAmmo = maxExplosiveCannonballAmmo;
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

    public void respawn()
    {
        cannonballAmmo = maxCannonballAmmo;
        explosiveCannonballAmmo = maxExplosiveCannonballAmmo;
        health = maxHealth;
    }

    public void resupply()
    {
        health = Math.Min(health + regenFactorOfMaxHealth * maxHealth, maxHealth);
        cannonballAmmo = Math.Min(cannonballAmmo + regenOfCannonballsPerReloadPeriod, maxCannonballAmmo);
        explosiveCannonballAmmo = Math.Min(explosiveCannonballAmmo + regenOfExplosiveCannonballPerReloadPeriod, maxExplosiveCannonballAmmo);
    }
}
