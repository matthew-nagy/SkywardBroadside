using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationSpeed;

    Vector3 rotationVector;

    // Start is called before the first frame update
    void Start()
    {
        rotationVector = Vector3.up;
        rotationSpeed *= 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationVector * rotationSpeed * Time.deltaTime);
    }
}
