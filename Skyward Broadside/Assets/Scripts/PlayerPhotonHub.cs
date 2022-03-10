using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class PlayerPhotonHub : PhotonTeamsManager
{
    // THIS IS PUBLIC FOR NOW, TO ALLOW FINE TUNING DURING TESTING EASIER.
    public float forceToDamageMultiplier = 0.1f;

    private float currHealth;
    private float cannonBallAmmo;
    private float explosiveAmmo;
    private int currentWeapon;

    private int deaths;

    //Allow single player testing
    bool disabled;

    //The actual ship of the player
    private GameObject PlayerShip;
    private GuiUpdateScript updateScript;

    public List<Material> teamMaterials;
    public int myTeam = -1;

    private DateTime gameStartTime;
    private TimeSpan gameLength = TimeSpan.FromSeconds(360f); //6 mins

    private bool gotScores = false;

    public void SetTeam(int team)
    {
        myTeam = team;
        Material givenMaterial = teamMaterials[team];
        if(givenMaterial == null)
        {
            Debug.LogError("Material was null");
        }
        transform.Find("Ship").transform.Find("Body").GetComponent<Renderer>().material = givenMaterial;
    }

    // Start is called before the first frame update
    void Start()
    {
        //var properties = new Hashtable();
        //properties.Add("deaths", deaths);
        //PhotonNetwork.SetPlayerCustomProperties(properties);
        
        PlayerShip = this.gameObject.transform.GetChild(0).gameObject;

        GameObject userGUI = GameObject.Find("User GUI");
        Debug.Log(userGUI);
        if(userGUI != null)
        {
            Debug.Log("Inside the if part with the value " + userGUI);
            updateScript = userGUI.GetComponent<GuiUpdateScript>();
            disabled = false;
            // We would want a way of accessing the players ship, and fetching the max health of only that. Probably could do it with an enum or something
            currHealth = PlayerShip.GetComponent<ShipArsenal>().maxHealth;
            updateScript.UpdateGUIHealth(currHealth);
            cannonBallAmmo = PlayerShip.GetComponent<ShipArsenal>().maxCannonballAmmo; 
            explosiveAmmo = PlayerShip.GetComponent<ShipArsenal>().maxExplosiveCannonballAmmo;
            updateScript.UpdateGUIAmmo(cannonBallAmmo);
            updateScript.UpdateGUIExplosiveAmmo(explosiveAmmo);
            currentWeapon = PlayerShip.GetComponentInChildren<BasicCannonController>().currentWeapon;
            UpdateWeapon(currentWeapon);
            FetchScores();
        }
        else
        {
            disabled = true;
            Debug.LogWarning("No User GUI could be found (player photon hub constructor)");
        }
        UpdateTimerFromMaster();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStartTime == DateTime.MinValue)
        {
            UpdateTimerFromMaster();
        }
        else
        {
            UpdateTimer();
        }

        if (!gotScores)
        {
            FetchScores();
        }
    }

    public void UpdateScores(int[] scores)
    {
        if (myTeam == 1)
        {
            updateScript.UpdateGUIScores(scores[0], scores[1]);
        }
        else
        {
            updateScript.UpdateGUIScores(scores[1], scores[0]);
        }
    }

    public void FetchScores()
    {
        var properties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (properties.ContainsKey("score"))
        {
            var scores = (int[]) properties["score"];
            UpdateScores(scores);
            gotScores = true;
        }
    }

    public void UpdateHealth(float collisionMagnitude)
    {
        float healthVal = collisionMagnitude * forceToDamageMultiplier;
        currHealth -= healthVal;
        if (currHealth < 0)
        {
            die();
        }
        if (updateScript == null)
        {
            Debug.LogWarning("Cannot update health on gui: photon hub's update script is null");
        }
        else
        {
            updateScript.UpdateGUIHealth(currHealth);
        }
    }

    private void die()
    {
        // update player death count
        var properties = new Hashtable();
        properties.Add("deaths", ++deaths);
        PhotonNetwork.SetPlayerCustomProperties(properties);

        int content = myTeam;

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        PhotonNetwork.RaiseEvent(1, myTeam, raiseEventOptions, SendOptions.SendReliable);
        
        // respawn
        currHealth = PlayerShip.GetComponent<ShipArsenal>().maxHealth;
        cannonBallAmmo = PlayerShip.GetComponent<ShipArsenal>().maxCannonballAmmo; 
        explosiveAmmo = PlayerShip.GetComponent<ShipArsenal>().maxExplosiveCannonballAmmo;

        PlayerShip.transform.position = new Vector3(Random.Range(-25, 25), 5f, Random.Range(-25, 25));
        
        Debug.Log(deaths);
    }

    public void UpdateAmmo(string type, float ammoLevel)
    {
        if (updateScript != null)
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
        else
        {
            Debug.LogWarning("Cannot update cannons: photon hubs update script is null");
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
        if (updateScript != null)
        {
            updateScript.UpdateWeapon(weapon);
        }
        else
        {
            Debug.LogWarning("Cannot update weapons on gui, photon hub'su update script is null");
        }
    }

    public void UpdateTimerFromMaster()
    {
        var roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        
        gameStartTime = roomProperties.ContainsKey("startTime") ? DateTime.Parse((string) roomProperties["startTime"]) : DateTime.MinValue;
    }

    private void UpdateTimer()
    {
        DateTime endTime = gameStartTime.Add(gameLength);
        TimeSpan timeRemaining = endTime.Subtract(DateTime.Now);
        
        updateScript.UpdateTimer(timeRemaining);
        if (timeRemaining < TimeSpan.Zero)
        {
            disabled = true;
            updateScript.gameOverScreen.SetActive(true);
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        Debug.LogFormat("RECEIVED EVENT {0}", photonEvent.Code);
        byte eventCode = photonEvent.Code;
        if (eventCode == 1)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            int team = (int) photonEvent.CustomData;
            var properties = PhotonNetwork.CurrentRoom.CustomProperties;
            int[] scores = properties.ContainsKey("scores") ? (int[]) properties["scores"] : new int[] {0, 0};
            scores[team] += 1;

            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(2, scores, raiseEventOptions, SendOptions.SendReliable); 
        }
        else if (eventCode == 2)
        {
            Debug.Log("RECEIVED UPDATE TO UPDATE SCORES");
            var scores = (int[])photonEvent.CustomData;
            UpdateScores(scores);    
        }
    }
}