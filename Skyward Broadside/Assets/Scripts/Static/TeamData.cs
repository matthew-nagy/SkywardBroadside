using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Static class for controlling data to do with teams
public static class TeamData
{
    //Lovely enum to get rid of strings in the code
    public enum Team : byte
    {
        Purple = 0, Yellow = 1
    }

    //Occasionally... we do need strings in the code
    const string purpleString = "Purple";
    const string yellowString = "Yellow";

    //Get the colour for the purple team
    static Color PurpleColour()
    {
        return new Color(1.0f, 0.0f, 1.0f);
    }

    //Get the colour for the yellow team
    static Color YellowColour()
    {
        return new Color(1.0f, 1.0f, 0.0f);
    }

    //Get the string for any given team
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

    //Get the team from any given string
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

    //Get the colour from and given team
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

    //Get the team from any given colour
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

        //Enetered a bad colour, send purple so it doesn't crash and go debug it
        Debug.LogError("Invalid colour entered");
        return Team.Purple;
    }
}
