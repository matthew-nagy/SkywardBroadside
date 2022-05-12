using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Scoreboard : MonoBehaviourPunCallbacks
{
    public static Scoreboard Instance;
    [SerializeField] private GameObject container;
    [SerializeField] public GameObject team1Panel;
    [SerializeField] public GameObject team2Panel;

    [SerializeField] private ScoreboardListing scoreboardListingPrefab;

    private Dictionary<string, ScoreboardListing> _listings;

    // Awake is called when the script instance is loaded
    void Awake()
    {
        Instance = this;
        _listings = new Dictionary<string, ScoreboardListing>();
        container.SetActive(false);

    }

    // keybindings for showing and unshowing scoreboard
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) // change to use Matt's thing once it is merged
        {
            container.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            container.SetActive(false);
        }
    }

    // Called from the initialisation of PlayerController to add that player to the scoreboard
    public void OnNewPlayer(PlayerController pc)
    {
        Transform panel = pc.myTeam == TeamData.Team.Purple ? team1Panel.transform : team2Panel.transform;
        ScoreboardListing listing = Instantiate(scoreboardListingPrefab, panel);
        listing.SetFromPlayerController(pc);
        _listings.Add(pc.playerName, listing);
    }
    
    // Delete entry from scoreboard when a player leaves
    public override void OnPlayerLeftRoom(Player player)
    {
       Destroy(_listings[player.NickName].gameObject);
       _listings.Remove(player.NickName);
    }

    // Update entry on scoreborad for player
    public void Recollect(string name)
    {
        _listings[name].SetFromPlayerController(photonHub.players[name]);
        Debug.Log("RECOLLECTING FOR " + name);
    }
}

