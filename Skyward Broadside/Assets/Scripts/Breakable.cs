using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public float breakForce;
    public float collisionMultiplier;
    public bool broken;
    public float mass;

    public BreakMaster owner;

    void Start()
    {
        owner.IncrimentBreakables();    
    }

    private void OnDestroy()
    {
        owner.RegisterBreakableDestroyed();
    }

    private void Update()
    {
        if (owner.HasShattered() && !broken)
        {
            //transform.Rotate(new Vector3(0, 1, 0) * Time.deltaTime);
            _break(breakForce * owner.shatterStrength, gameObject.GetComponent<Rigidbody>(), "Shattered", transform.position + Vector3.up * 0.01f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("Collision");
        if (!broken)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            float impulse = collision.impulse.magnitude;
            if (collision.gameObject.tag == "Ship")
            {
                Vector3 velocityBeforeCollision = collision.gameObject.GetComponent<ShipController>().velocityBeforeCollision;

                //Currently assuming ship loses 20% of its speed in collision
                //Then multiplying by some constant in an attempt to get it to actually destroy some of the island lol
                impulse = collision.rigidbody.mass * 0.2f * velocityBeforeCollision.magnitude * 1000f;
            }
            _break(impulse, rb, collision.collider.tag, collision.contacts[0].point);
        }
    }

    public void _break(float impulseMagnitude, Rigidbody rb, string colliderTag, Vector3 contactPoint)
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
                if (colliderTag == "RegularProjectile" || colliderTag == "Ship")
                {
                    print("impact");
                    rb.AddExplosionForce(impulseMagnitude * collisionMultiplier, contactPoint, 2);
                }
                owner.DecrimentBreakables();
            }

        }
    }
}