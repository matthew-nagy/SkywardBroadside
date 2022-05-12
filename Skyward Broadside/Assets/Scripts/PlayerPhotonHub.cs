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

//Contains code taken from https://doc.photonengine.com/en-us/pun/current/demos-and-tutorials/pun-basics-tutorial/player-ui-prefab
public class PlayerPhotonHub : MonoBehaviour
{
    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    public GameObject PlayerUiPrefab;

    public TeamData.Team myTeam;

    public string playerName { get; set; }
    public float playerID { get; set; }
    bool clientRegisteredID = false;

    //Allow single player testing
    bool disabled;

    public GUIController updateScript;

    public List<Material> teamMaterials;

    private int deaths;

    private DateTime gameStartTime;
    private TimeSpan gameLength = TimeSpan.FromSeconds(410f); //6 mins + intro

    private bool gotScores = false;

    //The game object containing this player's healthbar and nametag.
    public GameObject healthbarAndName;

    Transform ship;

    public void SetTeam()
    {
        string shipType = transform.root.Find("Ship").GetChild(0).transform.name;
        myTeam = transform.Find("Ship").Find(shipType).GetComponent<PlayerController>().myTeam;
        Material givenMaterial = teamMaterials[(int)myTeam];
        if(givenMaterial == null)
        {
            Debug.LogError("Material was null");
        }
        ship = transform.Find("Ship");
        ship.transform.Find(shipType).Find("Body").GetComponent<Renderer>().material = givenMaterial;
        Debug.LogWarning("Player Team Set in Photon Hub");
    }

    private void Awake()
    {
        //Get the player name from the last game, if it is being stored (we decided not to store it)
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

        GameObject userGUI = GameObject.Find("User GUI");
        Debug.Log(userGUI);
        if (userGUI != null)
        {
            Debug.Log("Inside the if part with the value " + userGUI);
            updateScript = userGUI.GetComponent<GUIController>();
            disabled = false;
        }
        else
        {
            disabled = true;
            Debug.LogWarning("No User GUI could be found (player photon hub constructor)");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Blackboard.playerPhotonHub = this;

        //Instantiate UI (username and health)
        if (PlayerUiPrefab != null)
        {
            GameObject _uiGo = Instantiate(PlayerUiPrefab);
            //Send a message to instance we created
            //Requires receiver, will be alerted if no component to respond to it
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            _uiGo.GetComponent<PlayerUI>().SetTarget(this);
        }
        else
        {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }

        GameObject.Find("Game Manager").GetComponent<GameManager>().serverPlayerPhotonHub = this;

        //When SceneManager.sceneLoaded is invoked, the OnSceneLoaded function will be called
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //Sets the player's name
    public void SetName(string name)
    {
        playerName = name;
    }

    //Will be called when the scene is loaded.
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        OnLevelWasLoaded(scene.buildIndex);
    }

    //Deprecated in Unity 5.4 but kept for backwards compatibility
    private void OnLevelWasLoaded(int level)
    {
        //Instantiate player UI (username and health)
        GameObject _uiGo = Instantiate(this.PlayerUiPrefab);

        //Tell the player UI that this is the PlayerPhotonHub script of the player
        _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
    }

    //Set this player's healthbar and nametag game object.
    public void SetUI(GameObject UI)
    {
        healthbarAndName = UI;
    }
}