using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectGenerator : MonoBehaviour
{
    bool spawedEffect;

    private void Update()
    {
        spawedEffect = false;
    }

    public void SpawnEffect(Vector3 pos, ParticleSystem effect)
    {
        if (!spawedEffect)
        {
            spawedEffect = true;
            Instantiate(effect, pos, Quaternion.identity);
        }
    }
}
