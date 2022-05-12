using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A class used to play various miscellaneous sound effects. The functions in this class are called by other classes when a sound effect needs to be played.
public class MiscSFXController : MonoBehaviour
{
    public AudioClip victorySting;
    public AudioClip defeatSting;
    public AudioClip lowHealth;
    public AudioClip leadTaken;
    public AudioClip leadLost;
    public AudioClip cantShoot;

    AudioSource audioSource;

    // Start is called before the first frame update.
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    //Plays the victory jingle for the user.
    public void PlayVictorySting()
    {
        audioSource.PlayOneShot(victorySting, 1f);
    }

    //Plays the defeat jingle for the user.
    public void PlayDefeatSting()
    {
        audioSource.PlayOneShot(defeatSting, 1f);
    }

    //Plays the low health alarm for the user.
    public void PlayLowHealth()
    {
        audioSource.PlayOneShot(lowHealth, 1f);

    }

    //Plays the sound alerting the user that their team has taken the lead.
    public void PlayLeadTaken()
    {
        audioSource.PlayOneShot(leadTaken, 1f);

    }

    //Plays the sound alerting the user that their team has lost the lead.
    public void PlayLeadLost()
    {
        audioSource.PlayOneShot(leadLost, 1f);

    }

}
