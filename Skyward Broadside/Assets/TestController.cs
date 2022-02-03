using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
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
        velocity = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {



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
        print("Collision2");

        Vector3 initialVelocity = velocityBeforeCollision;
        float massA = rigidBody.mass;

        Vector3 colliderInitialVelocity = collision.transform.GetComponent<ShipController>().velocityBeforeCollision;
        float massB = collision.rigidbody.mass;


        Vector3 finalVelocity = ((massA - massB) / (massA + massB)) * initialVelocity + (2 * massB / (massA + massB)) * colliderInitialVelocity;

        velocity = finalVelocity;
    }
}
