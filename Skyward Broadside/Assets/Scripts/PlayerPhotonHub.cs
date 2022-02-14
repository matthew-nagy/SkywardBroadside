using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class PlayerPhotonHub : PhotonTeamsManager
{
    // THIS IS PUBLIC FOR NOW, TO ALLOW FINE TUNING DURING TESTING EASIER.
    public float forceToDamageMultiplier;

    private float currHealth;
    private float cannonBallAmmo;
    private float explosiveAmmo;
    private int currentWeapon;

    private int deaths;

    //The actual ship of the player
    private GameObject PlayerShip;
    private GuiUpdateScript updateScript;

    // Start is called before the first frame update
    void Start()
    {
        var properties = new Hashtable();
        properties.Add("deaths", deaths);
        PhotonNetwork.SetPlayerCustomProperties(properties);
        
        PlayerShip = this.gameObject.transform.GetChild(0).gameObject;
        updateScript = GameObject.Find("User GUI").GetComponent<GuiUpdateScript>();

        // We would want a way of accessing the players ship, and fetching the max health of only that. Probably could do it with an enum or something
        currHealth = PlayerShip.GetComponent<ShipArsenal>().maxHealth;
        cannonBallAmmo = PlayerShip.GetComponent<ShipArsenal>().maxCannonballAmmo; 
        explosiveAmmo = PlayerShip.GetComponent<ShipArsenal>().maxExplosiveCannonballAmmo;
        updateScript.UpdateGUIHealth(currHealth);
        updateScript.UpdateGUIAmmo(cannonBallAmmo);
        updateScript.UpdateGUIExplosiveAmmo(explosiveAmmo);
        currentWeapon = PlayerShip.GetComponentInChildren<BasicCannonController>().currentWeapon;
        UpdateWeapon(currentWeapon);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateScores();
    }

    public void UpdateScores()
    {
        int myTeamScore = 0;
        int enemyTeamScore = 0;
        var myTeam = PhotonNetwork.LocalPlayer.GetPhotonTeam();
        foreach ( var player in PhotonNetwork.CurrentRoom.Players)
        {
            var team = player.Value.GetPhotonTeam();
            int playersScore = (int)player.Value.CustomProperties["deaths"];
            Debug.Log(playersScore);
            // playersScore contains the players number of deaths and so must be added to the 
            // opposing teams score
            if (myTeam == team)
            {
                enemyTeamScore += playersScore;
            }
            else
            {
                myTeamScore += playersScore;
            }
        }
        updateScript.UpdateGUIScores(myTeamScore, enemyTeamScore);
    }

    public void UpdateHealth(float collisionMagnitude)
    {
        float healthVal = collisionMagnitude * forceToDamageMultiplier;
        currHealth -= healthVal;

        if (currHealth < 0)
        {
            die();
        }
        updateScript.UpdateGUIHealth(currHealth);
    }

    private void die()
    {
        // update player death count
        var properties = new Hashtable();
        properties.Add("deaths", ++deaths);
        PhotonNetwork.SetPlayerCustomProperties(properties);           
        
        // respawn
        currHealth = PlayerShip.GetComponent<ShipArsenal>().maxHealth;
        cannonBallAmmo = PlayerShip.GetComponent<ShipArsenal>().maxCannonballAmmo; 
        explosiveAmmo = PlayerShip.GetComponent<ShipArsenal>().maxExplosiveCannonballAmmo;

        PlayerShip.transform.position = new Vector3(Random.Range(-25, 25), 5f, Random.Range(-25, 25));
        
        Debug.Log(deaths);
    }

    public void UpdateAmmo(string type, float ammoLevel)
    {
        switch (type)
        {
            case "Cannonball":
                updateScript.UpdateGUIAmmo(ammoLevel);
                break;
            case "ExplosiveCannonball":
                updateScript.UpdateGUIExplosiveAmmo(ammoLevel);
                break;
            default:
                throw new ArgumentException("Invalid string value for type");
        }
    }

    public void UpdateWeapon(int newWeapon)
    {
        currentWeapon = newWeapon; 
        string weapon = currentWeapon switch
        {
            0 => "Normal",
            1 => "Explosive",
            _ => throw new ArgumentException("Invalid weapon number"),
        };
        updateScript.UpdateWeapon(weapon);
    }
}
