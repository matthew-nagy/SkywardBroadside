using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    [SerializeField]
    ParticleSystem explosion;

    public bool isActive;
    bool didExplosion;
    float startTime;

    private void Start()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - startTime > 2f)
        {
            isActive = false;
        }

        if (isActive && !didExplosion)
        {
            if (Random.Range(0f, 1000f) < 1f)
            {
                DoExplosion();
            }
        }
    }

    void DoExplosion()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
        didExplosion = true;
    }
}
