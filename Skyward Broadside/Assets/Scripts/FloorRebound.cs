using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorRebound : MonoBehaviour
{
    public float reboundVelocity = 20f;
    GameObject center;

    private void Start()
    {
        center = GameObject.Find("Center");   
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        ShipController ship = obj.GetComponent<ShipController>();
        if(ship == null)
        {
            return;
        }
        ship.ResetLastPosition();
        ship.DisableMovementFor(1f);
        ship.velocity = new Vector3(ship.velocity.x, reboundVelocity, ship.velocity.z);
    }
}
