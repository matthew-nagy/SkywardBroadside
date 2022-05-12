using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseTeam : MonoBehaviour
{
    // Joins the Order of the guard
    public void OnClick_joinOrder()
    {
        joinTeam(TeamData.Team.Yellow);
    }

    // Joins Sky Rats
    public void OnClick_joinSR()
    {
        joinTeam(TeamData.Team.Purple);
    }

    // Sets the player's team in different static classes and then loads the choose ship scene
    public void joinTeam(TeamData.Team team)
    {
        PlayerChoices.team = team;
        TeamButton.joinTeam = team;
        SceneManager.LoadScene("Choose Ship");
    }
}
