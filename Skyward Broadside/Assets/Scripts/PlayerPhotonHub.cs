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

public class PlayerPhotonHub : PhotonTeamsManager, IPunObservable
{
    // THIS IS PUBLIC FOR NOW, TO ALLOW FINE TUNING DURING TESTING EASIER.
    public float forceToDamageMultiplier = 0.2f;

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    public GameObject PlayerUiPrefab;

    public string playerName { get; set; }

    public float currHealth { get; set; }
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
    public List<Color> teamColours;
    public int myTeam = -1;

    private DateTime gameStartTime;
    private TimeSpan gameLength = TimeSpan.FromSeconds(360f); //6 mins

    // time used in respawn invincibility
    private DateTime spawnTime;

    // spawn positions
    private Vector3 redSpawn = new Vector3(300f, 5f, -400f);
    private Vector3 blueSpawn = new Vector3(-160f, 5f, -80f);

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
        
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnTime = System.DateTime.Now;
        var properties = new Hashtable();
        properties.Add("deaths", deaths);
        PhotonNetwork.SetPlayerCustomProperties(properties);

       
        
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

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
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
    }

    public void UpdateScores()
    {
        if (disabled) {
            return;
        }
        int myTeamScore = 0;
        int enemyTeamScore = 0;
        var myTeam = PhotonNetwork.LocalPlayer.GetPhotonTeam();
        foreach ( var player in PhotonNetwork.CurrentRoom.Players)
        {
            var team = player.Value.GetPhotonTeam();
            var properties = player.Value.CustomProperties;
            int playersScore = properties.ContainsKey("deaths") ? (int) properties["deaths"] : 0;
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

    public void RegenInvoker()
    {
        InvokeRepeating("NearBaseRegen", 0, 1f);
    }
    public void NearBaseRegen()
    {
        if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Name == "Red")
        {
            if (Vector3.Distance(PlayerShip.transform.position, redSpawn) < 120)
            {
                currHealth = Math.Min(currHealth + 0.05f * PlayerShip.GetComponent<ShipArsenal>().maxHealth, PlayerShip.GetComponent<ShipArsenal>().maxHealth);
                updateScript.UpdateGUIHealth(currHealth);
                PlayerShip.GetComponent<ShipArsenal>().cannonballAmmo = Math.Min(PlayerShip.GetComponent<ShipArsenal>().cannonballAmmo + 3,PlayerShip.GetComponent<ShipArsenal>().maxCannonballAmmo);
                PlayerShip.GetComponent<ShipArsenal>().explosiveCannonballAmmo = Math.Min(PlayerShip.GetComponent<ShipArsenal>().explosiveCannonballAmmo + 1, PlayerShip.GetComponent<ShipArsenal>().maxExplosiveCannonballAmmo);
                updateScript.UpdateGUIAmmo(PlayerShip.GetComponent<ShipArsenal>().cannonballAmmo);
                updateScript.UpdateGUIExplosiveAmmo(PlayerShip.GetComponent<ShipArsenal>().explosiveCannonballAmmo);
            }
        }
        else if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Name == "Blue")
        {
            if (Vector3.Distance(PlayerShip.transform.position, blueSpawn) < 120)
            {
                currHealth = Math.Min(currHealth + 0.05f * PlayerShip.GetComponent<ShipArsenal>().maxHealth, PlayerShip.GetComponent<ShipArsenal>().maxHealth);
                updateScript.UpdateGUIHealth(currHealth);
                PlayerShip.GetComponent<ShipArsenal>().cannonballAmmo = Math.Min(PlayerShip.GetComponent<ShipArsenal>().cannonballAmmo + 3, PlayerShip.GetComponent<ShipArsenal>().maxCannonballAmmo);
                PlayerShip.GetComponent<ShipArsenal>().explosiveCannonballAmmo = Math.Min(PlayerShip.GetComponent<ShipArsenal>().explosiveCannonballAmmo + 1, PlayerShip.GetComponent<ShipArsenal>().maxExplosiveCannonballAmmo);
                updateScript.UpdateGUIAmmo(PlayerShip.GetComponent<ShipArsenal>().cannonballAmmo);
                updateScript.UpdateGUIExplosiveAmmo(PlayerShip.GetComponent<ShipArsenal>().explosiveCannonballAmmo);
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
    }

    private void die()
    {
        // update player death count
        var properties = new Hashtable();
        properties.Add("deaths", ++deaths);
        PhotonNetwork.SetPlayerCustomProperties(properties);

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        PhotonNetwork.RaiseEvent(1, null, raiseEventOptions, SendOptions.SendReliable);
        
        // respawn
        currHealth = PlayerShip.GetComponent<ShipArsenal>().maxHealth;
        cannonBallAmmo = PlayerShip.GetComponent<ShipArsenal>().maxCannonballAmmo; 
        explosiveAmmo = PlayerShip.GetComponent<ShipArsenal>().maxExplosiveCannonballAmmo;

        if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Name == "Red")
        {
            PlayerShip.transform.position = redSpawn + new Vector3(Random.Range(-80, 80), 0, Random.Range(-80, 80));
        }
        else if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Name == "Blue")
        {
            PlayerShip.transform.position = blueSpawn + new Vector3(Random.Range(-80, 80), 0, Random.Range(-80, 80));
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
            UpdateScores();
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
        }
        else
        {
            currHealth = (float)stream.ReceiveNext();
        }
    }
}