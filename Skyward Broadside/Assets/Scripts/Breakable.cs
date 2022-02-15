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
        if (!broken)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            _break(collision.impulse.magnitude, rb, collision.collider.tag, collision.contacts[0].point);
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
                if (colliderTag == "RegularProjectile")
                {
                    print("impact");
                    rb.AddExplosionForce(impulseMagnitude * collisionMultiplier, contactPoint, 2);
                }
                owner.DecrimentBreakables();
            }

        }
    }
}