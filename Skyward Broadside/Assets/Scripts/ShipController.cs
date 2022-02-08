using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{

    Rigidbody rigidBody;

    float moveSpeed;
    float turnSpeed = 20f;
    //float thrust;
    float topSpeed = 7f;

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

    // Update is called once per frame
    void Update()
    {
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

    }


}