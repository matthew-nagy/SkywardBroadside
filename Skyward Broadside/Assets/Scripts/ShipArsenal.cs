using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipArsenal : MonoBehaviour
{
    public float maxCannonballAmmo;
    public float maxExplosiveCannonballAmmo;

    public float cannonballAmmo;
    public float explosiveCannonballAmmo;

    public List<GameObject> equippedWeapons;

    public float maxHealth;

    private void Start()
    {
        cannonballAmmo = maxCannonballAmmo;
        explosiveCannonballAmmo = maxExplosiveCannonballAmmo;
    }
}
