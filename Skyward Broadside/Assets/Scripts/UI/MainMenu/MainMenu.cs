using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void exit()
    {
        Application.Quit();
    }

    public void options()
    {
        SceneManager.LoadScene("Options Menu");
    }

    public void play()
    {
        SceneManager.LoadScene("rooms");
    }
}
