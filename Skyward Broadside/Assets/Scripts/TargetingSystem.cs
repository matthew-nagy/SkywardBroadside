using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TargetingSystem : MonoBehaviourPunCallbacks
{
    GameObject currentTarget;
    public int currentTargetId;
    public Vector3 freeFireTargetPos;
    bool targetAquired;
    public bool lockedOn;
    public LayerMask layerMask;
    public float maxTargetDistance; //for free fire only atm
    public bool targetDestroyed;
    public Cinemachine.CinemachineFreeLook myCam;

    private void Start()
    {
        if (photonView.IsMine)
        {
            gameObject.layer = 2;
            transform.Find("Weapons").gameObject.layer = 2;
            transform.Find("Weapons").Find("GatlingGun").gameObject.layer = 2;
            transform.Find("Body").gameObject.layer = 2;
            transform.Find("Body").Find("CannonBay").gameObject.layer = 2;
            transform.Find("Body").Find("CannonBay").Find("Cannon1").gameObject.layer = 2;
            transform.Find("Body").Find("CannonBay").Find("Cannon2").gameObject.layer = 2;
            transform.Find("Body").Find("CannonBay").Find("Cannon3").gameObject.layer = 2;
            transform.Find("Body").Find("CannonBay").Find("Cannon4").gameObject.layer = 2;
            transform.Find("Body").Find("CannonBay").Find("Cannon5").gameObject.layer = 2;
            transform.Find("Body").Find("CannonBay").Find("Cannon6").gameObject.layer = 2;
            transform.Find("Body").Find("BalloonLeft").gameObject.layer = 2;
            transform.Find("Body").Find("BalloonRight").gameObject.layer = 2;
            transform.Find("Body").Find("Propeller").gameObject.layer = 2;
            transform.Find("Body").Find("Propeller").Find("Blade2").gameObject.layer = 2;
            transform.Find("Body").Find("Propeller").Find("Blade1").gameObject.layer = 2;
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (currentTarget == null)
            {
                targetAquired = lockedOn = false;
            }

            getInput();

            if (targetAquired)
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
                    currentTargetId = closestEnemy.gameObject.GetComponent<PhotonView>().ViewID;
                    highlightTarget(closestEnemy);
                }
            }
        }
    }

    void getInput()
    {
        if (SBControls.lockOn.IsHeld())
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
        currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineColor = Color.red;
    }

    public void unLockToTarget()
    {
        lockedOn = false;
        GetComponent<CameraController>().setLookAtTarget(transform.gameObject);
        GetComponent<CameraController>().enableFreeCam();
        currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineColor = Color.yellow;
    }

    void checkVisible(GameObject currentTarget)
    {
        RaycastHit hit;
        if (Physics.Linecast(transform.position, currentTarget.transform.position, out hit, layerMask))
        {
            if (hit.collider.gameObject != currentTarget)
            {
                targetAquired = false;
                currentTargetId = 0;
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
            targetAquired = false;
            currentTargetId = 0;
            currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineWidth = 0;
        }
    }

    GameObject findClosestEnemyInView()
    {
        float shortestDist = float.PositiveInfinity;
        GameObject closestEnemy = null;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Ship");

        TeamData.Team myTeam = GetComponent<PlayerController>().myTeam;

        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerController>().myTeam != myTeam)
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
        }
        return closestEnemy;
    }

    void highlightTarget(GameObject closestEnemy)
    {
        currentTarget = closestEnemy;
        currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineWidth = 5;
    }

    //cast a ray from the camera forwards to find a object to shoot at. If no object hit by ray, fire a default distance in that direction
    public void aquireFreeFireTarget()
    {
        if (photonView.IsMine)
        {
            Transform camTransform = myCam.gameObject.transform;
            RaycastHit hit;
            if (Physics.Raycast(ray: camTransform.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), hitInfo: out hit, layerMask: layerMask, maxDistance: maxTargetDistance))
            {
                freeFireTargetPos = hit.point;
            }
            else
            {
                Vector3 fireDir = (camTransform.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))).direction;
                freeFireTargetPos = transform.position + (fireDir * 20);
            }
        }

    }
}
