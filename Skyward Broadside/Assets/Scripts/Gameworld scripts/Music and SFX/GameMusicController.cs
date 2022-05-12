using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class that controls the in-game music.
public class GameMusicController : MonoBehaviour
{
    public AudioClip battleMusic;
    public AudioClip gameOverMusic;
    AudioSource audioSource;
    static GameMusicController instance;
    public float lerpFactor = 0.7f;

    //Called when the script instance is being loaded.
    //Sets the audio clip to be the battle music so that this can be played once the match starts.
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = battleMusic;
        audioSource.volume = 0.2f;
    }

    //Called when the game object becomes enabled. In this case, this happens after the introduction.
    //Thus, the battle music begins playing after the introduction.
    void OnEnable()
    {
        audioSource.Play();
    }

    //Stops the battle music.
    public void DisableBattleMusic()
    {
        audioSource.Stop();
    }

    //Sets the audio clip to the game over music and plays the game over music.
    public void EnableGameOverMusic()
    {
        audioSource.volume = 0.6f;
        audioSource.clip = gameOverMusic;
        audioSource.Play();
    }
}
