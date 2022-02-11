using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TestController : MonoBehaviourPun
{
    Rigidbody rigidBody;

    float moveSpeed = 2f;
    float turnSpeed = 20f;
    //float thrust;
    float topSpeed = 7f;

    float angle;
    Vector3 velocity;
    public Vector3 velocityBeforeCollision;
    Vector3 turnDirection;
    float acceleration = 1f;
    float deceleration = 1f;
    bool isDisabled;
    float timerDisabled;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            //PlayerManager.LocalPlayerInstance.controller = this;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, MAYBE NOT USEFUL OUTSIDE OF TUTORIAL?
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        if (!isDisabled)
        {
            velocity = transform.forward * moveSpeed;
        }
        else
        {
            velocityBeforeCollision = velocity;

            if (timerDisabled < 3.0f)
            {
                timerDisabled += Time.deltaTime;
            }
            else
            {
                isDisabled = false;
                timerDisabled = 0f;
            }

        }

        getInput();
    }

    void getInput()
    {
        if (Input.GetKey(KeyCode.Mouse1) && transform.GetComponent<WeaponsController>().freeCamEnabled && !transform.GetComponent<WeaponsController>().weaponCamEnabled)
        {
            transform.GetComponent<WeaponsController>().enableRightSideWeapons();
        }
        else if (!Input.GetKey(KeyCode.Mouse1) && !transform.GetComponent<WeaponsController>().freeCamEnabled && transform.GetComponent<WeaponsController>().weaponCamEnabled)
        {
            transform.GetComponent<WeaponsController>().disableRightSideWeapons();
        }
    }

    private void FixedUpdate()
    {
        Quaternion deltaRotation = Quaternion.Euler(turnDirection * turnSpeed * Time.fixedDeltaTime);
        rigidBody.MoveRotation(rigidBody.rotation * deltaRotation);

        rigidBody.MovePosition(rigidBody.position + velocity * Time.deltaTime);
        velocityBeforeCollision = velocity;


    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.LogFormat("COLLISION with {0}", collision.gameObject.name);
        if (!photonView.IsMine)
        {
            return;
        }

        //if (!collision.gameObject.name.Contains("Ball"))
        //{
        //    return;
        //}


        if (collision.gameObject.tag == "Ship")
        {
            //print("Collision");
            print(collision.gameObject.name);

            Vector3 initialVelocity = velocityBeforeCollision;
            float massA = rigidBody.mass;
            Vector3 centreA = rigidBody.position;

            Vector3 colliderInitialVelocity = collision.transform.GetComponent<ShipController>().velocityBeforeCollision;
            float massB = collision.rigidbody.mass;
            Vector3 centreB = collision.rigidbody.position;
            print("Collider's velocity: " + colliderInitialVelocity);
            print("Collider's centre: " + centreB);
            print("Collider's mass: " + massB);


            Vector3 finalVelocity = initialVelocity - (2 * massB / (massA + massB)) * (Vector3.Dot(initialVelocity - colliderInitialVelocity, centreA - centreB) / Vector3.SqrMagnitude(centreA - centreB)) * (centreA - centreB);
            finalVelocity = 0.8f * finalVelocity;
            print("Final velocity: " + finalVelocity);
            moveSpeed = finalVelocity.magnitude;

            velocity = finalVelocity;

            isDisabled = true;
        }

    }
}
