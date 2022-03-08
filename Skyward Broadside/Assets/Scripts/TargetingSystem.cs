using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TargetingSystem : MonoBehaviourPunCallbacks
{
    GameObject currentTarget;
    bool targetAquired;
    bool lockedOn;
    public LayerMask layerMask;

    public Cinemachine.CinemachineFreeLook myCam;

    private void Start()
    {
        if (photonView.IsMine)
        {
            gameObject.layer = 2;
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (lockedOn)
            {
                checkVisible(currentTarget);
            }
            else
            {
                GameObject closestEnemy = findClosestEnemyInView();
                if (targetAquired)
                {
                    lockOn(closestEnemy);
                }
            }
        }
    }

    void checkVisible(GameObject currentTarget)
    {
        GameObject closestEnemy = findClosestEnemyInView();
        RaycastHit hit;
        if (Physics.Linecast(transform.position, currentTarget.transform.position, out hit, layerMask))
        {
            if (hit.collider.gameObject != currentTarget || currentTarget != closestEnemy)
            {
                lockedOn = false;
                targetAquired = false;
                currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineWidth = 0;
            }
        }
    }

    GameObject findClosestEnemyInView()
    {
        float shortestDist = float.PositiveInfinity;
        GameObject closestEnemy = null;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Ship");
        foreach (GameObject player in players)
        {
            Vector3 screenPoint = myCam.gameObject.GetComponent<Camera>().WorldToViewportPoint(player.transform.position);
            if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            {
                RaycastHit hit;
                if (Physics.Linecast(transform.position, player.transform.position, out hit))
                {
                    if (hit.collider.gameObject == player)
                    {
                        float dist = (player.transform.position - transform.position).magnitude;
                        if (dist < shortestDist)
                        {
                            shortestDist = dist;
                            closestEnemy = player;
                            targetAquired = true;
                        }
                    }
                }
            } 
        }
        return closestEnemy;
    }

    void lockOn(GameObject closestEnemy)
    {
        lockedOn = true;
        currentTarget = closestEnemy;
        currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineWidth = 10;
    }
}
