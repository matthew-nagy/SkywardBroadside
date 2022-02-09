using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ShipController : MonoBehaviourPun
{
    Rigidbody rigidBody;

    float moveSpeed;
    float turnSpeed = 20f;
    //float thrust;
    float topSpeed = 7f;
    public float currHealth = 100f;

    float angle;
    Vector3 velocity;
    public Vector3 velocityBeforeCollision;
    Vector3 turnDirection;
    float acceleration = 1f;
    float deceleration = 1f;
    //float smoothAccelerationPercentage;
    //float smoothDecelerationPercentage;

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

        //Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        //float inputMagnitude = inputDirection.magnitude;
        //float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
        //angle = Mathf.LerpAngle(angle, targetAngle, turnSpeed * Time.deltaTime);

        //velocity = transform.forward * moveSpeed * inputMagnitude;
        turnDirection = new Vector3(0, 1, 0) * horizontalInput;
        //angle = Mathf.LerpAngle(angle, turnDirection, turnSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.W))
        {
            moveSpeed = moveSpeed + acceleration * Time.fixedDeltaTime;
            velocity = transform.forward * moveSpeed;


        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveSpeed = moveSpeed + (-deceleration) * Time.fixedDeltaTime;
            velocity = transform.forward * moveSpeed;

        }

        moveSpeed = Mathf.Clamp(moveSpeed, 0, topSpeed);

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
        //rigidBody.MovePosition(rigidBody.position + velocity * Time.fixedDeltaTime);

        //if (Input.GetKeyDown(KeyCode.W))
        //{
        //    smoothDecelerationPercentage = 0f;
        //    smoothAccelerationPercentage += 0.2f;
        //    smoothAccelerationPercentage = Mathf.Clamp(smoothAccelerationPercentage, 0f, 1f);
        //    thrust = Mathf.Lerp(0f, 50f, smoothAccelerationPercentage);
        //    rigidBody.AddForce(transform.forward * thrust, ForceMode.Force);


        //}
        //else if (Input.GetKeyDown(KeyCode.S))
        //{
        //    smoothAccelerationPercentage = 0f;
        //    smoothDecelerationPercentage += 0.2f;
        //    smoothDecelerationPercentage = Mathf.Clamp(smoothDecelerationPercentage, 0f, 1f);
        //    thrust = -Mathf.Lerp(0f, 50f, smoothDecelerationPercentage);
        //    rigidBody.AddForce(transform.forward * thrust, ForceMode.Force);


        //}




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

        if (!collision.gameObject.name.Contains("Ball"))
        {
            //return;
        }


        if (collision.gameObject.tag == "Ship")
        {
            print("Collision");

            Vector3 initialVelocity = velocityBeforeCollision;
            float massA = rigidBody.mass;


            Vector3 colliderInitialVelocity = collision.transform.GetComponent<ShipController>().velocityBeforeCollision;
            float massB = collision.rigidbody.mass;


            Vector3 finalVelocity = ((massA - massB) / (massA + massB)) * initialVelocity + (2 * massB / (massA + massB)) * colliderInitialVelocity;
            print(finalVelocity);

            velocity = finalVelocity;
        }

        print("Jeepers");
        // Now that the ship has reacted to the collision, we can tell the player that a collision has occured, as this will impact health
        gameObject.GetComponentInParent<PlayerPhotonHub>().UpdateHealth(collision.impulse.magnitude);

    }


}