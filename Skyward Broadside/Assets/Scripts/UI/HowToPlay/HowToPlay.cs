using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlay : MonoBehaviour
{
    [SerializeField] private GameObject container;

    void Start()
    {
        container.SetActive(false);
    }

    public void closeHowToPlay()
    {
        container.SetActive(false);
    }
}
