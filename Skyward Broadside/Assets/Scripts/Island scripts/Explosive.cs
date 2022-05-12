//This script manages explosions (explosive cannonballs, missiles)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    public GameObject owner;

    public float explosionPower;
    public float explosionRadius;
    bool detonated;
    public LayerMask explodableObjects;
    [SerializeField]
    ParticleSystem explosion;

    GameObject effect;

    [SerializeField]
    GameObject explosionDebris;
    [SerializeField]
    GameObject explosionMetal;
    [SerializeField]
    GameObject explosionAir;

    //On collision with an exploadable obj. Set the appropiate explosion sound and detonate
    private void OnCollisionEnter(Collision collision)
    {
        if (!detonated && Physics.OverlapSphere(transform.position, explosionRadius, explodableObjects).Length > 0)
        {
            if (collision.collider.gameObject.CompareTag("Terrain") || collision.collider.gameObject.CompareTag("Debris"))
            {
                if (explosionDebris != null)
                {
                    effect = explosionDebris;
                }
            }
            else if (collision.collider.transform.root.childCount > 0)
            {
                if (collision.collider.transform.root.GetChild(0).childCount > 0)
                {
                    if (collision.collider.transform.root.GetChild(0).GetChild(0).CompareTag("Ship"))
                    {
                        if (explosionMetal != null)
                        {
                            effect = explosionMetal;
                        }
                    }
                    else
                    {
                        effect = explosionAir;
                    }
                }
                else
                {
                    effect = explosionAir;
                }
            }
            else
            {
                effect = explosionAir;
            }
            Detonate();
        }
    }

    //Create particle effect. Apply an explosive force to any exploadable objects in range
    public void Detonate()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
        detonated = true;
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, explosionRadius, explodableObjects);
        foreach (Collider collider in collidersInRange)
        {
            Breakable breakable = collider.GetComponent<Breakable>();
            if (breakable != null)
            {
                if (!breakable.broken)
                {
                    breakable.GamePlayBreakCommand(explosionPower, collider.ClosestPoint(transform.position), explosionRadius);
                }
                else
                {
                    breakable.GamePlayApplyForce(explosionPower, collider.ClosestPoint(transform.position), explosionRadius);
                }
            }
        }
        Destroy(gameObject);
    }

    //Do the appropiate soudn effect if applicable
    private void OnDestroy()
    {
        if (effect != null)
        {
            if (owner != null)
            {
                if (owner.name == "turret")
                {
                    owner.transform.Find("SoundFxHub").GetComponent<SoundFxHub>().DoEffect(effect, transform.position);
                }
                else
                {
                    owner.transform.root.Find("SoundFxHub").GetComponent<SoundFxHub>().DoEffect(effect, transform.position);
                }
            }
        }
        else
        {
            effect = explosionAir;
            if (owner != null)
            {
                if (owner.name == "turret")
                {
                    owner.transform.Find("SoundFxHub").GetComponent<SoundFxHub>().DoEffect(effect, transform.position);
                }
                else
                {
                    owner.transform.root.Find("SoundFxHub").GetComponent<SoundFxHub>().DoEffect(effect, transform.position);
                }
            }
        }
    }
}