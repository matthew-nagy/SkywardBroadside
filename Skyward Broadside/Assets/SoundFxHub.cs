using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFxHub : MonoBehaviour
{ 
    bool doEffect;
    GameObject Soundfx;
    Vector3 EffectPos;

    List<GameObject> effectObjs = new List<GameObject>();

    void FixedUpdate()
    {
        if (doEffect)
        {
            if (Soundfx != null)
            {
                doEffect = false;
                GameObject effect = Instantiate(Soundfx, EffectPos, Quaternion.identity);
                effectObjs.Add(effect);
                Invoke(nameof(DestroyEffectObj), 2f);
            }
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

    void DestroyEffectObj()
    {
        GameObject effect = effectObjs[0];
        effectObjs.Remove(effect);
        Destroy(effect);
    }
}
