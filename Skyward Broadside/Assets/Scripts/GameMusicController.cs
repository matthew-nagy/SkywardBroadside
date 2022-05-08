using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMusicController : MonoBehaviour
{
    public AudioClip battleMusic;
    public AudioClip gameOverMusic;
    AudioSource audioSource;
    static GameMusicController instance;
    public float lerpFactor = 0.7f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = battleMusic;
        audioSource.volume = 0.2f;
    }
    void OnEnable()
    {
        audioSource.Play();
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
