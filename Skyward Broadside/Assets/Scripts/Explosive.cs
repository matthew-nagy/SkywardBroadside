using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    public float explosionPower;
    public float explosionRadius;
    bool detonated;
    public LayerMask explodableObjects;

    private void Update()
    {
        getInput();
    }

    void getInput()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            detonate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!detonated && Physics.OverlapSphere(transform.position, explosionRadius, explodableObjects).Length > 0)
        {
            detonate();
        }
    }

    void detonate()
    {
        detonated = true;
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, explosionRadius, explodableObjects);
        foreach (Collider collider in collidersInRange)
        {
            Breakable breakable = collider.GetComponent<Breakable>();
            if (breakable != null)
            {
                if (!breakable.broken)
                {
                    breakable._break();
                }
                breakable.applyForce(collider.GetComponent<Rigidbody>(), explosionPower, collider.ClosestPoint(transform.position), explosionRadius);
            }
        }
        Destroy(gameObject);
    }
}