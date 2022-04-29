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

    // This handles making sure there's only one sound source playing the background music
    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("MenuBGMusic");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // This deletes the menu music when we don't need it anymore
    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("MenuBGMusic");

        if (objs.Length > 0 && scene.name == "Beta")
        {
            Destroy(this.gameObject);
        }
        
    }

    // updates current scene so it knows where it is
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        DontDestroyOnLoad(this.gameObject);
    }
}