using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipArsenal : MonoBehaviour
{
    public int maxCannonballAmmo;
    public int maxExplosiveCannonballAmmo;

    public int cannonballAmmo;
    public int explosiveCannonballAmmo;

    //Dictionary containing all weapons by Id and whether they are equipped on the ship or not
    //in future some script on the ship controller would equip certain weapons depending on the ship type?
    //for now we just equip all the weapons on start
    // 0 = regular cannon, 1 = explosive cannons
    public Dictionary<int, bool> weapons = new Dictionary<int, bool> { { 0, false }, { 1, false } };

    public float maxHealth;

    private void Start()
    {
        enableWeapon(0);
        enableWeapon(1);
        GetComponent<WeaponsController>().equipWeapons();

        cannonballAmmo = maxCannonballAmmo;
        explosiveCannonballAmmo = maxExplosiveCannonballAmmo;
    }

    void enableWeapon(int weaponId)
    {
        weapons[weaponId] = true;
    }
}
