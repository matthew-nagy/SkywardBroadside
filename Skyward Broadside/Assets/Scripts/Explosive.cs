using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    public float explosionPower;
    public float explosionRadius;
    bool detonated;
    public LayerMask explodableObjects;
    [SerializeField]
    ParticleSystem explosion;

    private void OnCollisionEnter(Collision collision)
    {
        if (!detonated && Physics.OverlapSphere(transform.position, explosionRadius, explodableObjects).Length > 0)
        {
            Detonate();
        }
    }

    void Detonate()
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
}