using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPhysics : MonoBehaviour
{
    float thrust;
    Rigidbody rigidBody;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {

            thrust = 50f;
            rigidBody.AddForce(transform.forward * thrust, ForceMode.Force);


        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            //smoothAccelerationPercentage = 0f;
            //smoothDecelerationPercentage += 0.2f;
            //smoothDecelerationPercentage = Mathf.Clamp(smoothDecelerationPercentage, 0f, 1f);
            //thrust = -Mathf.Lerp(0f, 50f, smoothDecelerationPercentage);
            //rigidBody.AddForce(transform.forward * thrust, ForceMode.Force);


        }
    }
}
