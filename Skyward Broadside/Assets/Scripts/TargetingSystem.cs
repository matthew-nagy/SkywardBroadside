using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TargetingSystem : MonoBehaviourPunCallbacks
{
    public GameObject currentTarget;
    bool targetAquired;
    bool targetOutlined;
    public bool lockedOn;
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
            getInput();

            if (targetOutlined)
            {
                checkVisible(currentTarget);
                if (!lockedOn)
                {
                    checkStillClosest(currentTarget);
                }
            }
            else
            {
                GameObject closestEnemy = findClosestEnemyInView();
                if (targetAquired)
                {
                    highlightTarget(closestEnemy);
                }
            }
        }
    }

    void getInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (!lockedOn && targetAquired)
            {
                lockOnToTarget(currentTarget);
            }
            else if (lockedOn)
            {
                unLockToTarget();
            }
        }
    }

    void lockOnToTarget(GameObject currentTarget)
    {
        lockedOn = true;
        GetComponent<CameraController>().setLookAtTarget(currentTarget);
        GetComponent<CameraController>().disableFreeCam();
        currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineColor = Color.green;
    }

    void unLockToTarget()
    {
        lockedOn = false;
        GetComponent<CameraController>().setLookAtTarget(transform.gameObject);
        GetComponent<CameraController>().setFollowTarget(transform.gameObject);
        GetComponent<CameraController>().enableFreeCam();
        currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineColor = Color.red;
    }

    void checkVisible(GameObject currentTarget)
    {
        RaycastHit hit;
        if (Physics.Linecast(transform.position, currentTarget.transform.position, out hit, layerMask))
        {
            if (hit.collider.gameObject != currentTarget)
            {
                targetOutlined = false;
                targetAquired = false;
                currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineWidth = 0;
                unLockToTarget();
            }
        }
    }

    void checkStillClosest(GameObject currentTarget)
    {
        GameObject closestEnemy = findClosestEnemyInView();
        if (currentTarget != closestEnemy)
        {
            targetOutlined = false;
            targetAquired = false;
            currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineWidth = 0;
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

    void highlightTarget(GameObject closestEnemy)
    {
        targetOutlined = true;
        currentTarget = closestEnemy;
        currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineWidth = 10;
    }
}
