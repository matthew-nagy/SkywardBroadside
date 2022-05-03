using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ReboundingShip
{
    public ShipController ship;
    public float timeRebounded;

    public ReboundingShip(ShipController theShip)
    {
        ship = theShip;
        timeRebounded = 0.0f;
    }
}

public class Arena : MonoBehaviour
{
    public float reboundVelocity = 30.0f;

    static float secondsUntilGiveUp = 1.5f;

    List<ReboundingShip> ships;

    void ReboundShip(ShipController ship)
    {
        ship.ResetLastPosition();
        ship.velocity = (transform.position - ship.transform.position).normalized * reboundVelocity;
        ship.DisableMovementFor(0.4f);
    }

    private void Start()
    {
        ships = new List<ReboundingShip>();
    }

    void OnTriggerExit(Collider other)
    {
        GameObject exiting = other.gameObject;
        ShipController ship = exiting.GetComponentInChildren<ShipController>();
        if(ship == null)
        {
            return; //Only care about ships
        }

        ReboundShip(ship);

        ships.Add(new ReboundingShip(ship));
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject exiting = other.gameObject;
        ShipController ship = exiting.GetComponentInChildren<ShipController>();
        if (ship == null)
        {
            return; //Only care about ships
        }

        foreach(ReboundingShip rs in ships)
        {
            if(rs.ship == ship)
            {
                ships.Remove(rs);
                break;
            }
        }
    }

    private void Update()
    {
        for(int i = 0; i < ships.Count; i++)
        {
            ReboundingShip rs = ships[i];
            ReboundShip(rs.ship);
            rs.timeRebounded += Time.deltaTime;
            if(rs.timeRebounded >= secondsUntilGiveUp)
            {
                ships.RemoveAt(i);
                i -= 1;
            }
        }
    }
}
