using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscSFXController : MonoBehaviour
{
    public AudioClip victorySting;
    public AudioClip defeatSting;
    public AudioClip lowHealth;
    public AudioClip leadTaken;
    public AudioClip leadLost;
    public AudioClip cantShoot;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayVictorySting()
    {
        audioSource.PlayOneShot(victorySting, 1f);
    }

    public void PlayDefeatSting()
    {
        audioSource.PlayOneShot(defeatSting, 1f);
    }

    public void PlayLowHealth()
    {
        audioSource.PlayOneShot(lowHealth, 1f);

    }

    public void PlayLeadTaken()
    {
        audioSource.PlayOneShot(leadTaken, 1f);

    }

    public void PlayLeadLost()
    {
        audioSource.PlayOneShot(leadLost, 1f);

    }

}