using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public float breakForce = 10;
    public float collisionMultiplier = 10;
    public bool broken;
    public float mass = 5;

    private void OnCollisionEnter(Collision collision)
    {
        if(!broken)
        {
            if (collision.impulse.magnitude >= breakForce)
            {
                broken = true;
                GameObject replacement = Instantiate(gameObject, transform.position, transform.rotation);
                Rigidbody rb = replacement.gameObject.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    replacement.gameObject.AddComponent<Rigidbody>();
                    rb = replacement.gameObject.GetComponent<Rigidbody>();
                    rb.mass = mass;
                    rb.useGravity = true;
                }

                rb.AddExplosionForce(collision.impulse.magnitude * collisionMultiplier, collision.contacts[0].point, 2);
                print(collision.impulse.magnitude);

                Destroy(gameObject);
            }
        }
    }
}
