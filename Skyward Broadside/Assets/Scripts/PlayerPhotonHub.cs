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

public class PlayerPhotonHub : MonoBehaviour
{
    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    public GameObject PlayerUiPrefab;

    public string playerName { get; set; }
    public float playerID { get; set; }
    bool clientRegisteredID = false;

    //Allow single player testing
    bool disabled;

    public GuiUpdateScript updateScript;

    public List<Material> teamMaterials;
    public List<Color> teamColours;
    public int myTeam = -1;

    private int deaths;

    private DateTime gameStartTime;
    private TimeSpan gameLength = TimeSpan.FromSeconds(360f); //6 mins

    private bool gotScores = false;

    public GameObject healthbarAndName;

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

        var properties = new Hashtable();
        properties.Add("deaths", deaths);
        PhotonNetwork.SetPlayerCustomProperties(properties);

        GameObject userGUI = GameObject.Find("User GUI");
        Debug.Log(userGUI);
        if(userGUI != null)
        {
            Debug.Log("Inside the if part with the value " + userGUI);
            updateScript = userGUI.GetComponent<GuiUpdateScript>();
            disabled = false;
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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    // Update is called once per frame
    void Update()
    {
        if (transform.Find("Ship").GetComponent<PhotonView>().IsMine)
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

    public void AddDeath()
    {
        // update player death count
        var properties = new Hashtable();
        properties.Add("deaths", ++deaths);
        PhotonNetwork.SetPlayerCustomProperties(properties);

        int content = myTeam;

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(1, myTeam, raiseEventOptions, SendOptions.SendReliable);
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

            int team = (int)photonEvent.CustomData;
            var properties = PhotonNetwork.CurrentRoom.CustomProperties;
            int[] scores = properties.ContainsKey("scores") ? (int[])properties["scores"] : new int[] { 0, 0 };
            scores[team] += 1;

            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(2, scores, raiseEventOptions, SendOptions.SendReliable);
        }
        else if (eventCode == 2)
        {
            var scores = (int[])photonEvent.CustomData;
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

    public void SetUI(GameObject UI)
    {
        healthbarAndName = UI;
    }
}