using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public BreakablePhotonInterface breakPhotonInterface;
    public float breakForce;
    public bool broken;
    Rigidbody myRigidBody;
    bool isMasterPhoton;
    int indexInOwner;
    BreakMaster owner;


    void Start()
    {
        breakPhotonInterface.children.Add(this);
    }

    private void OnDestroy()
    {
    }

    private void Update()
    {
    }

    public void RegisterOwner(BreakMaster newOwner)
    {
        owner = newOwner;
        isMasterPhoton = owner.IsPhotonMaster();
        indexInOwner = owner.RegisterNewChild(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Ignore islands
        if (collision.gameObject.layer == 7 || !isMasterPhoton)
        {
            return;
        }
        //print("Collision");
        float impactForce;
        if (collision.gameObject.tag == "Ship")
        {
            Vector3 velocityBeforeCollision = collision.gameObject.GetComponent<ShipController>().velocityBeforeCollision;

            impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > breakForce)
            {
                if (!broken)
                {
                    Break(impactForce, collision.GetContact(0).point, 2);
                }
                else
                {
                    applyForce(impactForce, collision.GetContact(0).point, 2);
                }
            }
        }
        else if (collision.gameObject.tag == "Cannonball")
        {
            impactForce = collision.impulse.magnitude;
            if (impactForce > breakForce)
            {
                if (!broken)
                {
                    Break(impactForce, collision.GetContact(0).point, 2);
                    SendBreakCommand(impactForce, collision.GetContact(0).point, 2);
                }
                else
                {
                    applyForce(impactForce, collision.GetContact(0).point, 2);
                    SendBreakCommand(impactForce, collision.GetContact(0).point, 2);
                }
            }
        }
    }

    public void PhotonBreakCommand(float force, Vector3 contactPoint, float forceRadius)
    {
        Break(force, contactPoint, forceRadius);
    }

    public void GamePlayBreakCommand(float force, Vector3 contactPoint, float forceRadius)
    {
        if (isMasterPhoton)
        {
            Break(force, contactPoint, forceRadius);
        }
    }

    void Break(float force, Vector3 contactPoint, float forceRadius)
    {
        broken = true;
        gameObject.AddComponent<Rigidbody>();
        myRigidBody = GetComponent<Rigidbody>();
        myRigidBody.mass = 5;
        myRigidBody.useGravity = true;

        applyForce(force, contactPoint, forceRadius);
    }

    //Tells the break master that this is now broken
    void SendBreakCommand(float force, Vector3 contactPoint, float forceRadius)
    {

    }

    public void GamePlayApplyForce(float force, Vector3 contactPoint, float forceRadius)
    {
        if (isMasterPhoton)
        {
            applyForce(force, contactPoint, forceRadius);
        }
    }
    void applyForce(float force, Vector3 contactPoint, float forceRadius)
    {
        myRigidBody.AddExplosionForce(force, contactPoint, forceRadius);
    }
}