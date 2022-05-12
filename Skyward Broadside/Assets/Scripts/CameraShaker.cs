using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

//Currently only two things that can shake the camera, firing the cannons and being hit
public enum CameraShakeEvent
{
    Fire, Hit
}

//Serialize this so it can be edited in the unity editor
//contains data on how to shake the camera and for how long
[System.Serializable]
public struct ShakeProfile
{
    public float noiseAmplitude;
    public float noiseFrequency;
    public float secondDuration;
}

//You can't edit dictionaries in editor, so create these funky tuples
[System.Serializable]
public struct EventToShakeMapper
{
    public CameraShakeEvent shakeEvent;
    public ShakeProfile profile;
}


public class CameraShaker : MonoBehaviour
{
    public List<EventToShakeMapper> eventMap;
    //Made from the eventMap on startup
    Dictionary<CameraShakeEvent, ShakeProfile> shakeTypes;
    public CinemachineFreeLook cinemachineFreeCamera;
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public bool freeCam;

    public void Start()
    {
        shakeTypes = new Dictionary<CameraShakeEvent, ShakeProfile>();
        foreach(EventToShakeMapper mapper in eventMap)
        {
            shakeTypes[mapper.shakeEvent] = mapper.profile;
        }
    }

    //Shake the camera based on some event
    public void DoShakeEvent(CameraShakeEvent incommingEvent)
    {
        StartCoroutine(nameof(Shake), incommingEvent);
    }

    //Shake the cameras
    public IEnumerator Shake(CameraShakeEvent incommingEvent)
    {
        //Get how you should shake and set it
        ShakeProfile profile = shakeTypes[incommingEvent];
        SetNoise(profile.noiseAmplitude, profile.noiseFrequency);
        //wait for a period of time, based on the event
        yield return new WaitForSeconds(profile.secondDuration);
        SetNoise(0, 0);
    }

    //Sets the noise profile for all of the cameras
    void SetNoise(float amplitude, float frequency)
    {
        if (freeCam)
        {
            for (int i = 0; i < 3; i++)
            {
                cinemachineFreeCamera.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = amplitude;
                cinemachineFreeCamera.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = frequency;
            }
        }
        else if (!freeCam)
        {
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = amplitude;
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = frequency;
        }
    }
}
