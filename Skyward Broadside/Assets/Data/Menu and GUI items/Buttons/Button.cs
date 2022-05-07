using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    GameObject buttonSoundHub;
    private void Start()
    {
        buttonSoundHub = GameObject.Find("ButtonSoundHub");
    }

    public void PlaySound()
    {
        buttonSoundHub.GetComponent<AudioSource>().Play();
    }
}
