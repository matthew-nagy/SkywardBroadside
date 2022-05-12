using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Script that manages the escape menu, and it's different functions
public class EscapeMenu : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private GameObject howToPlay;

    // Set to off by default
    void Start()
    {
        container.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            container.SetActive(true);
        }
    }

    // Resume button
    public void closeEscape()
    {
        container.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // How to Play button
    public void openHowToPlay()
    {
        container.SetActive(false);
        howToPlay.SetActive(true);
    }

    // Disconnect from the photon room and then load th emain menu
    public void exit()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Main Menu");
    }
    
}
