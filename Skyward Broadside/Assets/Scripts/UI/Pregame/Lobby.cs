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

    public void RefreshListing(PlayerStatus ps)
    {
        if (_listings.ContainsKey(ps.playerName))
        {
            _listings[ps.playerName].SetFromPlayerStatus(ps);
        }
    }

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

    public override void OnPlayerLeftRoom(Player player)
    {
        Debug.Log(player.NickName);
        Destroy(_listings[player.NickName].gameObject);
        _listings.Remove(player.NickName);
    }

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
