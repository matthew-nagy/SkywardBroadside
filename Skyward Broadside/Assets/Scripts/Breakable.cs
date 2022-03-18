using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public float breakForce;
    public bool broken;

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
            //_break(breakForce * owner.shatterStrength, gameObject.GetComponent<Rigidbody>(), "Shattered", transform.position + Vector3.up * 0.01f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Ignore islands
        if (collision.gameObject.layer == 7)
        {
            return;
        }
        float impactForce;
        if (collision.gameObject.tag == "Ship")
        {
            Vector3 velocityBeforeCollision = collision.gameObject.GetComponent<ShipController>().velocityBeforeCollision;

            impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > breakForce)
            {
                if (!broken)
                {
                    _break();
                }
                applyForce(GetComponent<Rigidbody>(), impactForce, collision.GetContact(0).point, 2);
            }
        }
        else if (collision.gameObject.tag == "Cannonball")
        {
            impactForce = collision.impulse.magnitude;
            if (impactForce > breakForce)
            {
                if (!broken)
                {
                    _break();
                }
                applyForce(GetComponent<Rigidbody>(), impactForce, collision.GetContact(0).point, 2);
            }
        }
    }

    public void _break()
    {
        broken = true;
        gameObject.AddComponent<Rigidbody>();
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.mass = 5;
        rb.useGravity = true;
    }

    public void applyForce(Rigidbody rb, float force, Vector3 contactPoint, float forceRadius)
    {
        rb.AddExplosionForce(force, contactPoint, forceRadius);
    }
}