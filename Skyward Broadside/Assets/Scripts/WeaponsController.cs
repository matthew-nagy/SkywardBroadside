using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponsController : MonoBehaviour
{
    public List<GameObject> allCannnos;
    public float thresholdAngle;

    bool lockedOn;

    private void Update()
    {
        lockedOn = GetComponent<TargetingSystem>().lockedOn;

        //not free firing, we are locked to a target
        if (lockedOn)
        {
            foreach (GameObject cannon in allCannnos)
            {
                //enable each cannon that has line of sight to our target and let it know we are lockedOn to a target
                if (checkLineOfSight(cannon))
                {
                    cannon.GetComponent<BasicCannonController>().cannonActive = true;
                    cannon.GetComponent<BasicCannonController>().lockedOn = true;
                }
                else
                {
                    cannon.GetComponent<BasicCannonController>().cannonActive = false;
                    cannon.GetComponent<BasicCannonController>().lockedOn = true;
                }
            }
        } //free firing, no target lock. 
        else
        {
            GetComponent<TargetingSystem>().aquireFreeFireTarget();
            foreach (GameObject cannon in allCannnos)
            {
                //enable each cannon that has line of sight to the object in our crosshair (free fire target), let it know we are not locked on.
                if (checkLineOfSight(cannon))
                {
                    cannon.GetComponent<BasicCannonController>().cannonActive = true;
                    cannon.GetComponent<BasicCannonController>().lockedOn = false;
                }
                else
                {
                    cannon.GetComponent<BasicCannonController>().cannonActive = false;
                    cannon.GetComponent<BasicCannonController>().lockedOn = false;
                }
            }
        }
    }

    bool checkLineOfSight(GameObject cannon)
    {
        GameObject target;
        Vector3 targetPos;
        Vector3 vecToTarget;
        if (lockedOn)
        {
            target = PhotonView.Find(GetComponent<TargetingSystem>().currentTargetId).gameObject;
            vecToTarget = target.transform.position - cannon.GetComponent<BasicCannonController>().shotOrigin.transform.position;
        }
        else
        {
            targetPos = GetComponent<TargetingSystem>().freeFireTargetPos;
            vecToTarget = targetPos - cannon.GetComponent<BasicCannonController>().shotOrigin.transform.position;
        }

        float angle = Vector3.Angle(cannon.GetComponent<BasicCannonController>().shotOrigin.forward, vecToTarget);
        if (angle > thresholdAngle)
        {
            return false;
        }
        return true;
    }
}