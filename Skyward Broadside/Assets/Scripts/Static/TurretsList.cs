using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TurretsList
{
    public static List<GameObject> aliveTurrets = new List<GameObject>();

    public static void AddTurret(GameObject turret)
    {
        aliveTurrets.Add(turret);
    }

    public static void RemoveTurret(GameObject turret)
    {
        aliveTurrets.Remove(turret);
    }
}
