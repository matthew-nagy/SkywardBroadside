// This code is from The Unity Documentation 
// https://docs.unity3d.com/ScriptReference/Object.DontDestroyOnLoad.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Object.DontDestroyOnLoad example.
//
// This script example manages the playing audio. The GameObject with the
// "music" tag is the BackgroundMusic GameObject. The AudioSource has the
// audio attached to the AudioClip.

public class DontDestroyAudio : MonoBehaviour
{

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("MenuBGMusic");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("MenuBGMusic");

        if (objs.Length > 0 && scene.name == "Beta")
        {
            Debug.LogError("Destroying gameObject");
            Destroy(this.gameObject);
        }
        else
        {
            Debug.LogError("data is " + objs.Length + " and our lad " + scene.name);
        }
        
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        DontDestroyOnLoad(this.gameObject);
    }
}