using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreboardListing : MonoBehaviour
{
    [SerializeField] private Text _nametext;
    [SerializeField] private Text _killstext;
    [SerializeField] private Text _deathstext;
    [SerializeField] private Text _pointstext;

    private string _nickName;
    private int _kills;
    private int _deaths;
    private int _score;

    public void SetFromPlayerController(PlayerController pc)
    {
        _nickName = pc.playerName;
        _kills = pc.kills;
        _deaths = pc.deaths;
        _score = pc.score;
        UpdateText();
    }

    private void UpdateText()
    {
        Debug.Log("NAME:" + _nickName);
        Debug.Log("KILLS: " + _kills.ToString());
        Debug.Log("DEATHS: " + _deaths.ToString());
        Debug.Log("SCORE: " + _score.ToString());
        Debug.Log("TEXT UPDATED");
        _nametext.text = _nickName;
        _killstext.text = _kills.ToString();
        _deathstext.text = _deaths.ToString();
        _pointstext.text = _score.ToString();
    }
}
