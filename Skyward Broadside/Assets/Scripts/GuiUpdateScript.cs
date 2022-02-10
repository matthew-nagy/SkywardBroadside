using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuiUpdateScript : MonoBehaviour
{
    public Text health;
    public Text normalAmmo;
    public Text explosiveAmmo;
    public Text weapon;

    public void UpdateGUIHealth(float healthVal)
    {
        // This will at some point have some complicated extra stuff for a more interesting GUI i.e. dial control
        // but this is simple atm
        health.text = healthVal.ToString();
    }

    public void UpdateGUIAmmo(float ammo)
    {
        normalAmmo.text = ammo.ToString();
    }
    public void UpdateGUIExplosiveAmmo(float ammo)
    { 
        explosiveAmmo.text = ammo.ToString(); 
    }

    public void UpdateWeapon(string weaponName)
    {
        weapon.text = weaponName;
    }

}
