using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamButton : MonoBehaviour
{
    public static TeamData.Team joinTeam;
    // Start is called before the first frame update
    void Start()
    {
        joinTeam = TeamData.Team.Purple;
        GetComponent<Image>().color = TeamData.TeamToColour(joinTeam);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Clicked()
    {
        if (joinTeam == TeamData.Team.Purple)
        {
            joinTeam = TeamData.Team.Yellow;
        }
        else if (joinTeam == TeamData.Team.Yellow)
        {
            joinTeam = TeamData.Team.Purple;
        }
        GetComponent<Image>().color = TeamData.TeamToColour(joinTeam);
    }
}
