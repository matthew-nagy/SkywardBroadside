using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class photonHub : MonoBehaviourPunCallbacks
{

    public GUIController updateScript;
    public TeamData.Team myTeam;

    public static Dictionary<string, PlayerController> players;

    private DateTime gameStartTime = DateTime.MinValue;
    private TimeSpan gameLength = TimeSpan.FromSeconds(410f); //6 mins + intro time

    private bool gotScores = false;
    private bool isGameOver = false;

    private bool disabled;

    void Start()
    {
        players = new Dictionary<string, PlayerController>();

        GameObject userGUI = GameObject.Find("User GUI");
        Debug.Log(userGUI);
        if (userGUI != null)
        {
            Debug.Log("Inside the if part with the value " + userGUI);
            updateScript = userGUI.GetComponent<GUIController>();
            disabled = false;
            FetchScores();
        }
        else
        {
            disabled = true;
            Debug.LogWarning("No User GUI could be found (player photon hub constructor)");
        }

        myTeam = PlayerChoices.team;
        UpdateTimerFromMaster();
    }


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
        if (myTeam == TeamData.Team.Purple)
        {
            updateScript.UpdateGUIScores(scores[1], scores[0]);
        }
        else
        {
            updateScript.UpdateGUIScores(scores[0], scores[1]);
        }
    }

    public void FetchScores()
    {
        var properties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (properties.ContainsKey("scores"))
        {
            var scores = (int[])properties["scores"];
            UpdateScores(scores);
            gotScores = true;
        }
    }
    public void UpdateTimerFromMaster()
    {
        var roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (roomProperties.ContainsKey("timeSet") && (bool)roomProperties["timeSet"])
        {
            gameStartTime = DateTime.Parse((string)roomProperties["startTime"]);
        }
    }

    private void UpdateTimer()
    {
        DateTime endTime = gameStartTime.Add(gameLength);
        TimeSpan timeRemaining = endTime.Subtract(DateTime.Now);
        timeRemaining -= TimeSpan.FromHours(timeRemaining.Hours);
        timeRemaining -= TimeSpan.FromDays(timeRemaining.Days);

        // Llewellyn's computer 

        if ((timeRemaining < TimeSpan.Zero || timeRemaining.Minutes > 10) && !isGameOver)
        {
            disabled = true;
            gameOver();
            isGameOver = true;
            timeRemaining = TimeSpan.Zero;
        }

        updateScript.UpdateTimer(timeRemaining);
    }

    private void gameOver()
    {
        updateScript.gameOverScreen.SetActive(true);
        updateScript.gameOverScreen.GetComponent<gameOverScreen>().CopyScoreboard();
        var properties = PhotonNetwork.CurrentRoom.CustomProperties;
        updateScript.GameOver();
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public enum EventCode : byte
    {
        DeathEvent = 1,
        UpdatedScores,
        KillEventWithNames,
    }

    private void BroadcastEvent(EventCode code, System.Object data)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)code, data, raiseEventOptions, SendOptions.SendReliable);
    }

    private void updateTeamScoreProperties(System.Object customData)
    {
        byte team = (byte)((TeamData.Team)customData);
        var properties = PhotonNetwork.CurrentRoom.CustomProperties;
        int[] scores = properties.ContainsKey("scores") ? (int[])properties["scores"] : new int[] { 0, 0 };
        scores[team] += 1;

        properties["scores"] = scores;
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)EventCode.UpdatedScores, scores, raiseEventOptions, SendOptions.SendReliable);
    }

    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == (byte)EventCode.DeathEvent)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                updateTeamScoreProperties(photonEvent.CustomData);
            }
        }
        else if (eventCode == (byte)EventCode.UpdatedScores)
        {
            var scores = (int[])photonEvent.CustomData;
            UpdateScores(scores);
        }
        else if (eventCode == (byte)EventCode.KillEventWithNames)
        {
            var names = (string[])photonEvent.CustomData;
            Debug.Log(names[0] + ' ' + names[1]);
            if (names[0] != "Turret" && names[0] != "Terrain" && names[0] != "Debris")
            {
                players[names[0]].kills += 1;
                players[names[0]].score += 100;
                Scoreboard.Instance.Recollect(names[0]);
            }

            players[names[1]].deaths += 1;
            Scoreboard.Instance.Recollect(names[1]);

            KillFeed.Instance.AddNewListing(names[0], names[1]);
        }
    }
}
