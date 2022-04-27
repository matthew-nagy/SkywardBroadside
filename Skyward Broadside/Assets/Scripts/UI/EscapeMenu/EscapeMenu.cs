using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    public void openHowToPlay()
    {
        container.SetActive(false);
        howToPlay.SetActive(true);
    }

    public void exit()
    {
        // Ricky's code here
    }
    
}
