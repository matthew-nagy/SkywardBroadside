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
        //GameObject userGUI = GameObject.Find("User GUI");
        //Debug.Log(userGUI);
        //if(userGUI != null)
        //{
        //    Debug.Log("Inside the if part with the value " + userGUI);
        //    updateScript = userGUI.GetComponent<GUIController>();
        //    disabled = false;
        //}
        //else
        //{
        //    disabled = true;
        //    Debug.LogWarning("No User GUI could be found (player photon hub constructor)");
        //}

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void SetName(string name)
    {
        playerName = name;
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