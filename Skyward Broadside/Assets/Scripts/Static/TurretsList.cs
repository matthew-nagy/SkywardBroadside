using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//A static class used to keep track of the missile turrets which are still alive.
public static class TurretsList
{
    //List of all alive turret game objects.
    //As this is static, any class can access this to e.g., loop through all alive turrets.
    public static List<GameObject> aliveTurrets = new List<GameObject>();

    //Allows a turret to be added to the list of alive turrets.
    public static void AddTurret(GameObject turret)
    {
        aliveTurrets.Add(turret);
    }

    //Removes a turret from the list of alive turrets.
    public static void RemoveTurret(GameObject turret)
    {
        aliveTurrets.Remove(turret);
    }
}
