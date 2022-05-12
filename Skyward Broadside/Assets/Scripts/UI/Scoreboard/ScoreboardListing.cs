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

    // Uses variables from player controller to set details of the ScoreboardListing text
    public void SetFromPlayerController(PlayerController pc)
    {
        _nickName = pc.playerName;
        _kills = pc.kills;
        _deaths = pc.deaths;
        _score = pc.score;
        UpdateText();
    }

    // sets the text in the prefab
    private void UpdateText()
    {
        _nametext.text = _nickName;
        _killstext.text = _kills.ToString();
        _deathstext.text = _deaths.ToString();
        _pointstext.text = _score.ToString();
    }
}
