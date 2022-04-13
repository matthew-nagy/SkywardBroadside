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

    string shipType;

    private void Start()
    {
        activationRadius = GetComponent<SphereCollider>().radius;
        shipType = transform.root.GetComponent<PlayerPhotonHub>().shipType;
    }

    private void OnTriggerEnter(Collider other)
    {
        Detonate(); 
    }

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
        Destroy(gameObject);
    }

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

    void ShockwaveShip(Collider collider)
    {
        GameObject ship = collider.gameObject;
        Vector3 dirToApplyForce = (ship.transform.position - transform.position).normalized;

        ship.GetComponent<ShipController>().velocity += dirToApplyForce * shockwavePower;
    }

    void ShockwaveProjectile(Collider collider)
    {
        Vector3 dirToApplyForce = (collider.transform.position - transform.position).normalized;
        collider.GetComponent<Rigidbody>().velocity += dirToApplyForce * shockwavePower;
    }
}
