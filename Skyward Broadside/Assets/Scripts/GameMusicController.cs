using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMusicController : MonoBehaviour
{
    public AudioClip battleMusic;
    public AudioClip gameOverMusic;
    AudioSource audioSource;
    static GameMusicController instance;
    public static float cutsceneVolume = 0.5f;
    public float lerpFactor = 0.7f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = battleMusic;
        audioSource.volume = 0.2f;

        instance = this;
        enabled = false;
    }
    public static void StartCutsceneAudio()
    {
        instance.audioSource.Play();
        instance.audioSource.volume = cutsceneVolume;
    }

    public static void FadeInFromCutscene()
    {
        instance.enabled = true;
    }

    private void Update()
    {
        audioSource.volume += Time.deltaTime * lerpFactor;
        if(audioSource.volume >= 0.2f)
        {
            //Set the audio and stop updating
            audioSource.volume = 0.2f;
            enabled = false;
        }
    }


    public void EnableGameOverMusic()
    {
        audioSource.volume = 0.6f;
        audioSource.clip = gameOverMusic;
        audioSource.Play();
    }

    public void DisableBattleMusic()
    {
        audioSource.Stop();
    }
}
