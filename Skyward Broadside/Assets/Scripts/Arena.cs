using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Needs to be a class to take a reference to it
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

//Used to keep ships inside the playfild
public class Arena : MonoBehaviour
{
    public float reboundVelocity = 30.0f;

    //Sometimes unity will play both exit and enter events in the same frame, causing it to never let you go
    //So we set a hard cutoff time. If you aren't back in the playfield by this point, something else went wrong
    static float secondsUntilGiveUp = 1.5f;

    //Ships being put back into the arena
    List<ReboundingShip> ships;

    //Disables the ship and sets its velocity to the center of the arena
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

    //If a ship leaves the arena, it rebounds and its velocity is set backwards
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

    //Once the ship enters the arena again, it is removed from the ship list
    private void OnTriggerEnter(Collider other)
    {
        GameObject exiting = other.gameObject;
        ShipController ship = exiting.GetComponentInChildren<ShipController>();
        if (ship == null)
        {
            Destroy(other.gameObject);
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
    
    //Every update, set any captured ship's velocity to the center of the arena
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
