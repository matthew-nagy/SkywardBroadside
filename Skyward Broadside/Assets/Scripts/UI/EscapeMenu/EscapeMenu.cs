using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeMenu : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private GameObject howToPlay;

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

    public void closeEscape()
    {
        container.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void openHowToPlay()
    {
        container.SetActive(false);
        howToPlay.SetActive(true);
    }

    public void exit()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Main Menu");
    }
    
}
