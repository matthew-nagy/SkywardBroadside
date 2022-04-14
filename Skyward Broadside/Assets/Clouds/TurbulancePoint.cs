using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurbulancePoint : MonoBehaviour
{
    //Displacement this update
    public Vector3 velocity;
    private Vector3 lastPosition;
    private void Start()
    {
        velocity = Vector3.zero;
        lastPosition = transform.position;
    }

    private void Update()
    {
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }
}
