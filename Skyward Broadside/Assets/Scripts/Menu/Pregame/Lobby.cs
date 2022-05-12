using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviourPunCallbacks
{
    public string worldname = "Beta";

    public static Lobby Instance;

    [SerializeField] private GameObject purplePanel;
    [SerializeField] private GameObject yellowPanel;

    [SerializeField] private LobbyListing lobbyListingPrefab;

    [SerializeField] private GameObject guardReadyText;
    [SerializeField] private GameObject srReadyText;

    GameObject localStatus;

    private bool switching = false;

    private Dictionary<string, LobbyListing> _listings;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        _listings = new Dictionary<string, LobbyListing>();

        localStatus = PlayerStatus.CreateLocal();

        //Ensure button has the correct text
        SetReadyUpText(PlayerChoices.team);

    }

    // Responds to keybindinngs for readying up and changing team and also checks if all players are ready to begin the game
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            ReadyUp();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ChangeTeam();
        }

        if (!switching && PhotonNetwork.IsMasterClient)
        {
            GameObject[] playerstatuses = GameObject.FindGameObjectsWithTag("playerStatus");
            foreach (var go in playerstatuses)
            {
                PlayerStatus ps = go.GetComponent<PlayerStatus>();
                if (!ps.ready)
                {
                    return;
                }
            }

            PhotonNetwork.LoadLevel(worldname);
            switching = true;
        }

    }

    // Gives different ready up text depending on team
    private void SetReadyUpText(TeamData.Team team)
    {
        if (team == TeamData.Team.Purple)
        {
            guardReadyText.SetActive(false);
            srReadyText.SetActive(true);
        }
        else
        {
            guardReadyText.SetActive(true);
            srReadyText.SetActive(false);
        }
    }

    // Sends photon event to alert other clients that the player has readied up
    public void ReadyUp()
    {
        Debug.Log("Readying Up");
        localStatus.GetComponent<PlayerStatus>().ReadyUp();
        RefreshListing(localStatus.GetComponent<PlayerStatus>());

        RaiseEventOptions reo = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        string name = localStatus.GetComponent<PlayerStatus>().playerName;
        PhotonNetwork.RaiseEvent((byte)EventCode.Ready, name, reo, SendOptions.SendReliable);
        Debug.Log("SENT EVENT");
    }

    // Switch team, sending photon nevent to alert other clients that a player has chosen to switch team
    public void ChangeTeam()
    {
        Debug.Log("Changing Team");
        TeamData.Team newTeam = PlayerChoices.team == TeamData.Team.Purple ? TeamData.Team.Yellow : TeamData.Team.Purple;
        PlayerChoices.team = newTeam;

        SetReadyUpText(newTeam);

        RaiseEventOptions reo = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        string name = localStatus.GetComponent<PlayerStatus>().playerName;
        PhotonNetwork.RaiseEvent((byte)EventCode.SwitchTeam, name, reo, SendOptions.SendReliable);
        Debug.Log("SENT EVENT");
    }

    // Update lobby listing from a PlayerStatus
    public void RefreshListing(PlayerStatus ps)
    {
        if (_listings.ContainsKey(ps.playerName))
        {
            _listings[ps.playerName].SetFromPlayerStatus(ps);
        }
    }

    // Create a new lobby listing from a playerstatus
    public void OnNewPlayer(PlayerStatus player)
    {
        Transform panel = player.team == TeamData.Team.Purple ? purplePanel.transform : yellowPanel.transform;
        LobbyListing listing = Instantiate(lobbyListingPrefab, panel);
        listing.SetFromPlayerStatus(player);
        _listings.Add(player.playerName, listing);
    }

    public enum EventCode : byte
    {
        Ready = 10,
        SwitchTeam,
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    // Event handler for readying up and switching teams
    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == (byte)EventCode.Ready)
        {
            string name = (string)photonEvent.CustomData;
            GameObject[] playerstatuses = GameObject.FindGameObjectsWithTag("playerStatus");
            Debug.Log(playerstatuses.Length.ToString());
            foreach (var go in playerstatuses)
            {
                PlayerStatus ps = go.GetComponent<PlayerStatus>();
                if (ps.playerName == name)
                {

                    ps.ReadyUp();
                    RefreshListing(ps);
                    return;
                }
            }
        }
        else if (eventCode == (byte)EventCode.SwitchTeam)
        {
            string name = (string)photonEvent.CustomData;
            GameObject[] playerstatuses = GameObject.FindGameObjectsWithTag("playerStatus");
            foreach (var go in playerstatuses)
            {
                PlayerStatus ps = go.GetComponent<PlayerStatus>();
                if (ps.playerName == name)
                {
                    TeamData.Team newTeam = ps.team == TeamData.Team.Purple ? TeamData.Team.Yellow : TeamData.Team.Purple;
                    ps.team = newTeam;
                    Destroy(_listings[ps.playerName].gameObject);
                    _listings.Remove(ps.playerName);
                    OnNewPlayer(ps);
                    return;
                }
            }
        }
    }

    // Deletes the lobby listing when a player leaves
    public override void OnPlayerLeftRoom(Player player)
    {
        Debug.Log(player.NickName);
        Destroy(_listings[player.NickName].gameObject);
        _listings.Remove(player.NickName);
    }

    // Destroy listing with name
    public void DestroyListing(string name)
    {
        if (_listings.ContainsKey(name))
        {
            if (_listings[name] != null && _listings[name].gameObject != null)
            {
                Destroy(_listings[name].gameObject);
            }
            _listings.Remove(name);
        }
    }
}
