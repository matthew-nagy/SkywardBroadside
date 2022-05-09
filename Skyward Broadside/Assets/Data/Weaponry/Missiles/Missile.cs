using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Missile : MonoBehaviour
{
    public GameObject owner { get; set; }
    public Transform targetTransform;
    public float speed = 2;
    public float rotationDampening = 1;

    public float damageAmount;

    bool lockedOn;

    public bool shouldExplode = false;
    public float explodeTimer = 10; //How many seconds to wait before timing out and exploding
    private float initTime;

    private bool initialised = false;

    void Start()
    {
        //Do nothing before we have a target
    }

    public void InitialiseMissile(Transform _targetTransform)
    {
        targetTransform = _targetTransform;
        initialised = true;
        initTime = Time.timeSinceLevelLoad;
    }

    void Update()
    {
        if (!initialised) return; //If not got a target, do nothing
        if (transform != null)
        {
            Vector3 dist_to_target = targetTransform.position - transform.position;
            Vector3 dir_to_target = dist_to_target.normalized;

            //Rotate to look at target - dampening controls speed of rotation
            var rotation = Quaternion.LookRotation(dist_to_target);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDampening);

            transform.position += transform.forward * speed * Time.deltaTime;

            if (Time.timeSinceLevelLoad - initTime > explodeTimer)
            {
                GetComponent<Explosive>().Detonate();
            }
        }
    }
}
