using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum CameraShakeEvent
{
    Fire, Hit
}

[System.Serializable]
public struct ShakeProfile
{
    public float noiseAmplitude;
    public float noiseFrequency;
    public float secondDuration;
}

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
    public CinemachineFreeLook cinemachineCamera;

    public void Start()
    {
        shakeTypes = new Dictionary<CameraShakeEvent, ShakeProfile>();
        foreach(EventToShakeMapper mapper in eventMap)
        {
            shakeTypes[mapper.shakeEvent] = mapper.profile;
        }
    }

    public void DoShakeEvent(CameraShakeEvent incommingEvent)
    {
        StartCoroutine("Shake", incommingEvent);
    }

    public IEnumerator Shake(CameraShakeEvent incommingEvent)
    {
        ShakeProfile profile = shakeTypes[incommingEvent];
        SetNoise(profile.noiseAmplitude, profile.noiseFrequency);
        yield return new WaitForSeconds(profile.secondDuration);
        SetNoise(0, 0);
    }

    void SetNoise(float amplitude, float frequency)
    {
        for(int i = 0; i < 3; i++)
        {
            cinemachineCamera.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = amplitude;
            cinemachineCamera.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = frequency;
        }
    }
}
