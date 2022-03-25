using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TeamData
{
    public enum Team
    {
        Purple = 0, Yellow = 1
    }

    const string purpleString = "Purple";
    const string yellowString = "Yellow";

    static Color PurpleColour()
    {
        return new Color(1.0f, 0.0f, 1.0f);
    }

    static Color YellowColour()
    {
        return new Color(1.0f, 1.0f, 0.0f);
    }

    static public string TeamToString(Team chosenTeam)
    {
        if(chosenTeam == Team.Purple)
        {
            return purpleString;
        }
        else
        {
            return yellowString;
        }
    }

    static public Team StringToTeam(string chosenString)
    {
        if(chosenString == purpleString)
        {
            return Team.Purple;
        }
        else
        {
            return Team.Yellow;
        }
    }

    static public Color TeamToColour(Team chosenTeam)
    {
        if(chosenTeam == Team.Purple)
        {
            return PurpleColour();
        }
        else
        {
            return YellowColour();
        }
    }

    static public Team ColourToTeam(Color inputColour)
    {
        if(inputColour == PurpleColour())
        {
            return Team.Purple;
        }
        else if(inputColour == YellowColour())
        {
            return Team.Yellow;
        }

        Debug.LogError("Invalid colour entered");
        return Team.Purple;
    }
}
