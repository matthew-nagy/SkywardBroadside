using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // ends the applicatiom
    public void exit()
    {
        Application.Quit();
    }

    // GO to the options menu
    public void options()
    {
        SceneManager.LoadScene("Options Menu");
    }

    // Go to the playername scene
    public void play()
    {
        SceneManager.LoadScene("PlayerName");
    }
}
