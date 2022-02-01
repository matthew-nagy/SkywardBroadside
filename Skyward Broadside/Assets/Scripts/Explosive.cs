using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    public float explosionPower = 300;
    public float explosionRadius = 5;
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
        if (!detonated)
        {
            if (collision.gameObject.name != "Barrel")
            {
                detonate();
            }
        }
    }

    void detonate()
    {
        detonated = true;
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, explosionRadius, explodableObjects);
        foreach (Collider collider in collidersInRange)
        {
            Rigidbody rb = collider.gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                collider.gameObject.AddComponent<Rigidbody>();
                rb = collider.gameObject.GetComponent<Rigidbody>();
                rb.mass = 5;
                rb.useGravity = true;
            }
            rb.AddExplosionForce(explosionPower, transform.position, explosionRadius);
        }
        Destroy(gameObject);
    }
}
