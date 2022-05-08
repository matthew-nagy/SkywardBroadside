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

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        _listings = new Dictionary<string, ScoreboardListing>();
        container.SetActive(false);

    }

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

    /*IEnumerator CreateListingsForAllPlayers()
    {
        yield return new WaitForSeconds(1f);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Ship");
        foreach (GameObject player in players)
        {
            PlayerPhotonHub pph = player.GetComponent<PlayerPhotonHub>();
            if (pph.myTeam == 0)
            {
                ScoreboardListing listing = Instantiate(scoreboardListingPrefab, team1Panel.transform);
                listing.SetFromPph(pph);
                _listings.Add(pph.playerName, listing);
            }
            else
            {
                ScoreboardListing listing = Instantiate(scoreboardListingPrefab, team2Panel.transform);
                listing.SetFromPph(pph);
                _listings.Add(pph.playerName, listing);
            }
        }
    }*/

    public void OnNewPlayer(PlayerController pc)
    {
        Transform panel = pc.myTeam == TeamData.Team.Purple ? team1Panel.transform : team2Panel.transform;
        ScoreboardListing listing = Instantiate(scoreboardListingPrefab, panel);
        listing.SetFromPlayerController(pc);
        _listings.Add(pc.playerName, listing);
    }
    
    
    public override void OnPlayerLeftRoom(Player player)
    {
       Destroy(_listings[player.NickName].gameObject);
       _listings.Remove(player.NickName);
    }

    public void Recollect(string name)
    {
        _listings[name].SetFromPlayerController(photonHub.players[name]);
        Debug.Log("RECOLLECTING FOR " + name);
    }
}

