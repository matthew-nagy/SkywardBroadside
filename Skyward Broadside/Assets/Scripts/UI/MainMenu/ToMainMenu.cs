using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Scipt that will load the Main Menu Scene
public class ToMainMenu : MonoBehaviour
{
    public void goToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
