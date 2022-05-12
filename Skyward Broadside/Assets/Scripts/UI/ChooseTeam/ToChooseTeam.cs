using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Scipt that will load the scene that allows the player to choose a team
public class ToChooseTeam : MonoBehaviour
{
    public void goToTeam()
    {
        SceneManager.LoadScene("ChooseTeam");
    }
}
