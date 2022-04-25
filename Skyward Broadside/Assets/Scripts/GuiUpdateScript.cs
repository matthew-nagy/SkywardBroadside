using System;
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
    public Text myScore;
    public Text otherScore;
    public Text timer;
    public GameObject gameOverScreen;
    public Text gameOverYourTeam;
    public Text gameOverOtherTeam;

    private float gameLength = 360f; //6 mins
    private float timeRemaining;

    private void Start()
    {
        timeRemaining = gameLength;
    }

    private void Update()
    {
    }

    public void UpdateGUIHealth(float healthVal)
    {
        // This will at some point have some complicated extra stuff for a more interesting GUI i.e. dial control
        // but this is simple atm
        health.text = Math.Ceiling(healthVal).ToString();
    }

    public void UpdateGUIAmmo(float ammo)
    {
        normalAmmo.text = ammo.ToString();
    }
    public void UpdateGUIExplosiveAmmo(float ammo)
    { 
        explosiveAmmo.text = ammo.ToString(); 
    }

    public void UpdateWeapon(int weaponId)
    {
        switch (weaponId)
        {
            case 0:
                weapon.text = "Cannonball";
                break;

            case 1:
                weapon.text = "Explosive cannonball";
                break;

            case 2:
                weapon.text = "Gatling gun";
                break;

            case 3:
                weapon.text = "Shockwave cannons";
                break;

            case 4:
                weapon.text = "Homing projectiles";
                break;

            default:
                Debug.LogError("Invalid weapon Id");
                break;
        }
    }

    public void UpdateGUIScores(int myTeam, int otherTeam)
    {
        myScore.text = myTeam.ToString();
        gameOverYourTeam.text = myTeam.ToString();
        
        otherScore.text = otherTeam.ToString();
        gameOverOtherTeam.text = otherTeam.ToString();
    }

    public void UpdateTimer(TimeSpan timeRemaining)
    {
        timer.text = String.Format("{0}:{1:00}", timeRemaining.Minutes, timeRemaining.Seconds);
    }
}
