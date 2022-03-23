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

    //Used to prevent Sync hell at the end of the game
    bool applicationQuit;

    static public float maxSecondsPerSyncEvent = 0.8f;
    private float secondsToNextSync;
    float secondsSinceSync = 0.0f;


    void Start()
    {
        breakPhotonInterface.children.Add(this);
        secondsToNextSync = 0.03F;
    }

    private void OnApplicationQuit()
    {
        applicationQuit = true;
    }

    private void OnDestroy()
    {
        if (isMasterPhoton)
        {
            //At the end of the game for example
            if (applicationQuit != true)
            {
                SendSyncCommand(true);
            }
        }
    }

    private void Update()
    {
        if(myRigidBody != null && isMasterPhoton)
        {
            secondsSinceSync += Time.deltaTime;
            if(secondsSinceSync >= secondsToNextSync)
            {
                secondsSinceSync = 0;
                SendSyncCommand(false);
                secondsToNextSync *= 1.5f;
                if(secondsToNextSync > maxSecondsPerSyncEvent)
                {
                    secondsToNextSync = maxSecondsPerSyncEvent;
                }
            }
        }
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
                    SendBreakCommand(impactForce, collision.GetContact(0).point, 2);
                }
                else
                {
                    applyForce(impactForce, collision.GetContact(0).point, 2);
                    SendSyncCommand(false);
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
                    SendSyncCommand(false);
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
            SendBreakCommand(force, contactPoint, forceRadius);
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
        BreakEvent be = new BreakEvent();
        be.indexInOwner = indexInOwner;
        be.force = force;
        be.contactPoint = contactPoint;
        be.forceRadius = forceRadius;

        owner.RegisterBreakEvent(be);
    }

    //Tells the break master where this componant is
    void SendSyncCommand(bool shouldDelete)
    {
        SyncEvent se = new SyncEvent();
        se.indexInOwner = indexInOwner;
        se.delete = shouldDelete;
        se.position = myRigidBody.position;
        se.velocity = myRigidBody.velocity;

        owner.RegisterSyncEvent(se);
    }

    public void PhotonSync(SyncEvent e)
    {
        transform.position = e.position;
        myRigidBody.position = e.position;
        myRigidBody.velocity = e.velocity;
    }

    public void GamePlayApplyForce(float force, Vector3 contactPoint, float forceRadius)
    {
        if (isMasterPhoton)
        {
            applyForce(force, contactPoint, forceRadius);
            SendSyncCommand(false);
        }
    }
    void applyForce(float force, Vector3 contactPoint, float forceRadius)
    {
        myRigidBody.AddExplosionForce(force, contactPoint, forceRadius);
    }
}