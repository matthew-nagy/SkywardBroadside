//This ship handles shockwave cannonballs, when they should detonate and their interactions with other game objects

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
    public float shockwavePower;
    float activationRadius;

    public LayerMask myLayerMask;
    public LayerMask otherLayerMask;
    public LayerMask shockwavableObjects;

    [SerializeField]
    ParticleSystem implosion;

    [SerializeField]
    GameObject shockwaveFx;

    public GameObject owner;

    //Set the activation radius of shockwave cannonballs (How close they have to get to an object to expload)
    private void Start()
    {
        activationRadius = GetComponent<SphereCollider>().radius;
    }

    //When close enough to an object, detonate and play sound
    private void OnTriggerEnter(Collider other)
    {
        Detonate();
        owner.transform.root.Find("SoundFxHub").GetComponent<SoundFxHub>().DoEffect(shockwaveFx, transform.position);
    }

    //"Detonate" find all shockwavable objects in range and handle the interaction
    void Detonate()
    {
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, activationRadius, shockwavableObjects);
        foreach (Collider collider in collidersInRange)
        { 
            if (collider.CompareTag("Terrain"))
            {
                ShockwaveTerrain(collider);
            }
            else if (collider.CompareTag("Ship"))
            {
                ShockwaveShip(collider);
            }
            else if (collider.CompareTag("Cannonball"))
            {
                ShockwaveProjectile(collider);
            }
            else if (collider.CompareTag("ExplosiveCannonball"))
            {
                ShockwaveProjectile(collider);
            }
        }

        owner.GetComponent<EffectGenerator>().SpawnEffect(transform.position, implosion);

        Destroy(gameObject);
    }

    //Apply a force to terrain objects in range
    void ShockwaveTerrain(Collider collider)
    {
        Breakable breakable = collider.GetComponent<Breakable>();
        if (breakable != null)
        {
            if (!breakable.broken)
            {
                breakable.GamePlayBreakCommand(shockwavePower * 0.5f, collider.ClosestPoint(transform.position), activationRadius);
            }
            else
            {
                breakable.GamePlayApplyForce(shockwavePower * 0.5f, collider.ClosestPoint(transform.position), activationRadius);
            }
        }
    }

    //Gice the ship a velocity away from the shockwave center
    void ShockwaveShip(Collider collider)
    {
        GameObject ship = collider.gameObject;
        Vector3 dirToApplyForce = (ship.transform.position - transform.position).normalized;

        ship.GetComponent<ShipController>().velocity += dirToApplyForce * shockwavePower;
    }

    //Propel any projectiles in range away from the shockwave center
    void ShockwaveProjectile(Collider collider)
    {
        Vector3 dirToApplyForce = (collider.transform.position - transform.position).normalized;
        collider.GetComponent<Rigidbody>().velocity += dirToApplyForce * shockwavePower;
    }
}
