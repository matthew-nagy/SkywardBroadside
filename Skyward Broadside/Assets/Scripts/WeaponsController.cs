using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponsController : MonoBehaviour
{
    public List<GameObject> allCannnos;
    public float thresholdAngle;

    bool weaponsHot;

    private void Update()
    {
        weaponsHot = GetComponent<TargetingSystem>().lockedOn;
        if (weaponsHot)
        {
            foreach (GameObject cannon in allCannnos)
            {
                if (checkLineOfSight(cannon))
                {
                    cannon.GetComponent<BasicCannonController>().cannonActive = true;
                }
                else
                {
                    cannon.GetComponent<BasicCannonController>().cannonActive = false;
                }
            }
        }
        else
        {
            foreach (GameObject cannon in allCannnos)
            {
                cannon.GetComponent<BasicCannonController>().cannonActive = false;
            }
        }
    }

    bool checkLineOfSight(GameObject cannon)
    {
        GameObject target = PhotonView.Find(GetComponent<TargetingSystem>().currentTargetId).gameObject;
        Vector3 vecToTarget = target.transform.position - cannon.GetComponent<BasicCannonController>().shotOrigin.transform.position;
        float angle = Vector3.Angle(cannon.GetComponent<BasicCannonController>().shotOrigin.forward, vecToTarget);
        if (angle > thresholdAngle)
        {
            return false;
        }
        return true;
    }
}