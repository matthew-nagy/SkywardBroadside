using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameOverScreen : MonoBehaviour
{
    [SerializeField] GameObject team1panel;
    [SerializeField] GameObject team2panel;

    public void CopyScoreboard()
    {
        foreach (Transform child in Scoreboard.Instance.team1Panel.transform)
        {
            Instantiate(child, team1panel.transform);
        }
        foreach (Transform child in Scoreboard.Instance.team2Panel.transform)
        {
            Instantiate(child, team2panel.transform);
        }
    }
}
