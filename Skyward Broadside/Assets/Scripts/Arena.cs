using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    public float reboundVelocity = 20.0f;

    void OnTriggerExit(Collider other)
    {
        GameObject exiting = other.gameObject;
        ShipController ship = exiting.GetComponentInChildren<ShipController>();
        if(ship == null)
        {
            return; //Only care about ships
        }

        ship.ResetLastPosition();
        ship.velocity = (transform.position - ship.transform.position).normalized * reboundVelocity;
        ship.DisableMovementFor(0.2f);
    }
}
