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
using UnityEngine.SceneManagement;

[System.Serializable]
public struct ReloadStation
{
    public GameObject gameObject;      //A game object so that, should the station move about, the position is still correct
    public float reloadRadius;      //How close the ship must be in order to begin reloading
}


public class PlayerPhotonHub : PhotonTeamsManager, IPunObservable
{
    // THIS IS PUBLIC FOR NOW, TO ALLOW FINE TUNING DURING TESTING EASIER.
    public float forceToDamageMultiplier = 0.2f;

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    public GameObject PlayerUiPrefab;

    public string playerName { get; set; }
    public float playerID { get; set; }
    bool clientRegisteredID = false;

    public float currHealth { get; set; }
    private float cannonBallAmmo;
    private float explosiveAmmo;
    private int currentWeapon;

    private int deaths;

    //Allow single player testing
    bool disabled;

    //The actual ship of the player
    private GameObject playerShip;
    private GuiUpdateScript updateScript;

    public List<Material> teamMaterials;
    public List<Color> teamColours;
    public int myTeam = -1;

    private DateTime gameStartTime;
    private TimeSpan gameLength = TimeSpan.FromSeconds(360f); //6 mins

    private bool gotScores = false;

    // time used in respawn invincibility
    private DateTime spawnTime;

    // spawn positions
    private Vector3 redSpawn = new Vector3(300f, 5f, -400f);
    private Vector3 blueSpawn = new Vector3(-160f, 5f, -80f);

    public float regenFactorOfMaxHealth = 0.05f;
    public int regenOfCannonballsPerReloadPeriod = 3;
    public int regenOfExplosiveCannonballPerReloadPeriod = 1;
    public float regenSecondsPerReloads = 1f;

    public List<ReloadStation> redReloadStations;
    public List<ReloadStation> blueReloadStations;

    public void SetTeam(int team)
    {
        myTeam = team;
        Material givenMaterial = teamMaterials[team];
        if(givenMaterial == null)
        {
            Debug.LogError("Material was null");
        }
        Transform ship = transform.Find("Ship");
        ship.gameObject.GetComponent<ShipController>().teamColour = teamColours[team];
        ship.transform.Find("Body").GetComponent<Renderer>().material = givenMaterial;
    }

    private void Awake()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerName = PlayerPrefs.GetString("PlayerName");
        }
        else
        {
            playerName = "Player";
        }

        if (PlayerPrefs.HasKey("UUID"))
        {
            playerID = PlayerPrefs.GetFloat("UUID");
        }
        else
        {
            Debug.LogError("Player does not have a universal unique ID");
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        Blackboard.playerPhotonHub = this;

        redReloadStations = new List<ReloadStation>();
        blueReloadStations = new List<ReloadStation>();

        spawnTime = System.DateTime.Now;
        var properties = new Hashtable();
        properties.Add("deaths", deaths);
        PhotonNetwork.SetPlayerCustomProperties(properties);

       
        
        playerShip = this.gameObject.transform.GetChild(0).gameObject;

        GameObject userGUI = GameObject.Find("User GUI");
        Debug.Log(userGUI);
        if(userGUI != null)
        {
            Debug.Log("Inside the if part with the value " + userGUI);
            updateScript = userGUI.GetComponent<GuiUpdateScript>();
            disabled = false;
            // We would want a way of accessing the players ship, and fetching the max health of only that. Probably could do it with an enum or something
            currHealth = playerShip.GetComponent<ShipArsenal>().maxHealth;
            updateScript.UpdateGUIHealth(currHealth);
            cannonBallAmmo = playerShip.GetComponent<ShipArsenal>().maxCannonballAmmo; 
            explosiveAmmo = playerShip.GetComponent<ShipArsenal>().maxExplosiveCannonballAmmo;
            updateScript.UpdateGUIAmmo(cannonBallAmmo);
            updateScript.UpdateGUIExplosiveAmmo(explosiveAmmo);
            currentWeapon = playerShip.GetComponentInChildren<BasicCannonController>().currentWeapon;
            UpdateWeapon(currentWeapon);
            FetchScores();
        }
        else
        {
            disabled = true;
            Debug.LogWarning("No User GUI could be found (player photon hub constructor)");
        }
        UpdateTimerFromMaster();

        //Instantiate UI (username and health)
        if (PlayerUiPrefab != null)
        {
            GameObject _uiGo = Instantiate(PlayerUiPrefab);

            //Send a message to instance we created
            //Requires receiver, will be alerted if no component to respond to it
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
 
        }
        else
        {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }

        GameObject.Find("Game Manager").GetComponent<GameManager>().serverPlayerPhotonHub = this;

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        //In 5 seconds, start repeating. This gives the game a chance to load photon stuff in
        Invoke("RegenInvoker", 5f);

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

    public void RegenInvoker()
    {
        InvokeRepeating("NearBaseRegen", 0, regenSecondsPerReloads);
    }

    void ReloadShipArsenal(ShipArsenal arsenal)
    {
        currHealth = Math.Min(currHealth + regenFactorOfMaxHealth * arsenal.maxHealth, arsenal.maxHealth);
        updateScript.UpdateGUIHealth(currHealth);
        arsenal.cannonballAmmo = Math.Min(arsenal.cannonballAmmo + regenOfCannonballsPerReloadPeriod, arsenal.maxCannonballAmmo);
        arsenal.explosiveCannonballAmmo = Math.Min(arsenal.explosiveCannonballAmmo + regenOfExplosiveCannonballPerReloadPeriod, arsenal.maxExplosiveCannonballAmmo);
        updateScript.UpdateGUIAmmo(arsenal.cannonballAmmo);
        updateScript.UpdateGUIExplosiveAmmo(arsenal.explosiveCannonballAmmo);
    }

    public void NearBaseRegen()
    {
        ShipArsenal arsenal = playerShip.GetComponent<ShipArsenal>();
        List<ReloadStation> reloadStations;
        if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Name == "Red")
        {
            reloadStations = redReloadStations;
        }
        else if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Name == "Blue")
        {
            reloadStations = blueReloadStations;
        }
        else
        {
            Debug.LogError("Player is trying to reload on neither red nor blue teams");
            reloadStations = new List<ReloadStation>();
        }

        foreach (ReloadStation station in reloadStations)
        {
            if (Vector3.Distance(playerShip.transform.position, station.gameObject.transform.position) <= station.reloadRadius)
            {
                ReloadShipArsenal(arsenal);
            }
        }
    }

    public void UpdateHealth(float collisionMagnitude)
    {
        // player gets 1 second of invincibility after joining the room and each time they respawn
        if ((System.DateTime.Now - spawnTime).TotalSeconds > 1)
        {
            float healthVal = collisionMagnitude * forceToDamageMultiplier;
            currHealth -= healthVal;
            if (currHealth <= 0.00)
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
    }

    private void die()
    {
        // update player death count
        var properties = new Hashtable();
        properties.Add("deaths", ++deaths);
        PhotonNetwork.SetPlayerCustomProperties(properties);

        int content = myTeam;

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        PhotonNetwork.RaiseEvent(1,myTeam, raiseEventOptions, SendOptions.SendReliable);
        
        // respawn
        currHealth = playerShip.GetComponent<ShipArsenal>().maxHealth;
        cannonBallAmmo = playerShip.GetComponent<ShipArsenal>().maxCannonballAmmo; 
        explosiveAmmo = playerShip.GetComponent<ShipArsenal>().maxExplosiveCannonballAmmo;

        if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Name == "Red")
        {
            playerShip.transform.position = redSpawn + new Vector3(Random.Range(-80, 80), 0, Random.Range(-80, 80));
        }
        else if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Name == "Blue")
        {
            playerShip.transform.position = blueSpawn + new Vector3(Random.Range(-80, 80), 0, Random.Range(-80, 80));
        }
        spawnTime = System.DateTime.Now;

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

    public void SetName(string name)
    {
        playerName = name;
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

        if (timeRemaining < TimeSpan.Zero)
        {
            disabled = true;
            updateScript.gameOverScreen.SetActive(true);
            timeRemaining = TimeSpan.Zero;
        }

        updateScript.UpdateTimer(timeRemaining);
        
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
            var scores = (int[]) photonEvent.CustomData;
            UpdateScores(scores);
        }
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        this.OnLevelWasLoaded(scene.buildIndex);
    }

    private void OnLevelWasLoaded(int level)
    {
        //Instantiate player UI (username and health)
        GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
        _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);

    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currHealth);
            stream.SendNext(playerID);
        }
        else
        {
            currHealth = (float)stream.ReceiveNext();
            playerID = (float)playerID;
            if (!clientRegisteredID)
            {
                Blackboard.registerPlayer(gameObject, playerID);
            }
        }
    }
}