using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFxHub : MonoBehaviour
{ 
    bool doEffect;
    GameObject Soundfx;
    Vector3 EffectPos;

    void FixedUpdate()
    {
        if (doEffect)
        {
            doEffect = false;
            Instantiate(Soundfx, EffectPos, Quaternion.identity);
        }
    }

    public void DoEffect(GameObject soundFx, Vector3 pos)
    {
        if (!doEffect)
        {
            doEffect = true;
            Soundfx = soundFx;
            EffectPos = pos;
        }
    }
}
