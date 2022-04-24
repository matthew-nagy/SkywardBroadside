using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListing : MonoBehaviour
{
    [SerializeField] private Text _nametext;
    [SerializeField] private Text _readytext;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetFromPlayerStatus(PlayerStatus ps)
    {
        _nametext.text = ps.playerName;
        _readytext.text = ps.ready ? "YES" : "NO";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
