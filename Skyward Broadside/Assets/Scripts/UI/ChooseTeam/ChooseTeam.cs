using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseTeam : MonoBehaviour
{
    public void OnClick_joinOrder()
    {
        joinTeam(TeamData.Team.Yellow);
    }

    public void OnClick_joinSR()
    {
        joinTeam(TeamData.Team.Purple);
    }

    public void joinTeam(TeamData.Team team)
    {
        PlayerChoices.team = team;
        TeamButton.joinTeam = team;
        SceneManager.LoadScene("Choose Ship");
    }
}
