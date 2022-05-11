using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used by a plane under the ocean so that debris doesn't last for the entire program lifetime
public class FloorCullSystem : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        //Don't destroy a player if they glitch below the map
        if (collision.gameObject.GetComponent<ShipController>() == null)
        {
            Destroy(collision.gameObject);
        }
        //If a player got below the ocean, throw them back above it and pretend that everything is OK :)
        collision.gameObject.transform.position = new Vector3(collision.gameObject.transform.position.x, 3f, collision.gameObject.transform.position.y);
    }
}
