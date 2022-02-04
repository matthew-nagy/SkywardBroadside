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

    bool isDisabled;
    float timerDisabled;
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

        if (!isDisabled)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            turnDirection = new Vector3(0, 1, 0) * horizontalInput;

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
        //print("Collision");

        Vector3 initialVelocity = velocityBeforeCollision;
        float massA = rigidBody.mass;
        Vector3 centreA = transform.position;

        Vector3 colliderInitialVelocity = collision.transform.GetComponent<TestController>().velocityBeforeCollision;
        float massB = collision.rigidbody.mass;
        Vector3 centreB = collision.transform.position;


        Vector3 finalVelocity = initialVelocity - (2 * massB / (massA + massB)) * (Vector3.Dot(initialVelocity - colliderInitialVelocity, centreA - centreB) / Vector3.SqrMagnitude(centreA - centreB)) * (centreA - centreB);
        finalVelocity = 0.8f * finalVelocity;
        moveSpeed = finalVelocity.magnitude;

        velocity = finalVelocity;

        isDisabled = true;
    }


}
