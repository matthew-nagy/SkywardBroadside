using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMusicController : MonoBehaviour
{
    public AudioClip battleMusic;
    public AudioClip gameOverMusic;
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = battleMusic;
        audioSource.volume = 0.2f;
    }
    private void OnEnable()
    {
        audioSource.Play();
    }
    
    public void EnableGameOverMusic()
    {
        audioSource.volume = 0.65f;
        audioSource.clip = gameOverMusic;
        audioSource.Play();
    }

    public void DisableBattleMusic()
    {
        audioSource.Stop();
    }
}
