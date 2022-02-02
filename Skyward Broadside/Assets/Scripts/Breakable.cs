using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public float breakForce;
    public float collisionMultiplier;
    public bool broken;
    public float mass;

    private void OnCollisionEnter(Collision collision)
    {
        if(!broken)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            _break(collision.impulse.magnitude, rb);
            rb = gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //rb.AddExplosionForce(collision.impulse.magnitude * collisionMultiplier, collision.contacts[0].point, 2);
            }
        }
    }

    public void _break(float impulseMagnitude, Rigidbody rb)
    {
        if (impulseMagnitude >= breakForce)
        {
            broken = true;
            if (rb == null)
            {
                gameObject.AddComponent<Rigidbody>();
                rb = gameObject.GetComponent<Rigidbody>();
                rb.mass = mass;
                rb.useGravity = true;
            }
        }
    }
}
