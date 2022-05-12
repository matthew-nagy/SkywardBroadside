//This script manages the lock on system. Aquiring targets, highlighting them etc.

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
    public float maxTargetDistance;
    public bool targetDestroyed;
    public Cinemachine.CinemachineFreeLook myCam;

    string shipType;

    [SerializeField]
    GameObject PromptSystem;
    [SerializeField]
    GameObject mpmPrefab;
    [SerializeField]
    string lockOnPrompt;
    [SerializeField]
    string firePrompt;
    GameObject mpmObj;
    MousePromptManager mpm;

    //Set ship to appropiate layer depending on if the photon view is ours
    private void Start()
    {
        if (photonView.IsMine)
        {
            MoveToLayer(transform, 2);
        }
        shipType = transform.root.Find("Ship").GetChild(0).transform.name;

        mpmObj = Instantiate(mpmPrefab);
        mpmObj.transform.parent = PromptSystem.transform;
        mpm = mpmObj.GetComponent<MousePromptManager>();
    }

    //Update is called once per frame
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
                checkAlive();
                if (!lockedOn)
                {
                    checkStillClosest(currentTarget);
                }
            }
            else
            {
                GameObject closestEnemy = FindClosestEnemyInView();
                if (targetAquired)
                {
                    currentTargetId = closestEnemy.gameObject.GetComponent<PhotonView>().ViewID;
                    highlightTarget(closestEnemy);
                    mpm.target = currentTarget;
                    mpm.promptText = lockOnPrompt;
                    mpm.MakePrompt();
                }
            }
        }
    }

    //Get player input to lock on or unlock to a target
    void getInput()
    {
        if (SBControls.lockOn.IsDown())
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

    //Move all applicable objects in the tree to the given layer
    void MoveToLayer(Transform root, int layer)
    {
        if (root.gameObject.layer != 13)
        {
            root.gameObject.layer = layer;
        }
        foreach (Transform child in root)
        {
            MoveToLayer(child, layer);
        }
    }

    //Set the targets outline to Red. Update the prompt for firing. Switch to the lock on camera
    void lockOnToTarget(GameObject currentTarget)
    {
        lockedOn = true;
        GetComponent<CameraController>().setLookAtTarget(currentTarget);
        GetComponent<CameraController>().disableFreeCam();
        currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineColor = Color.red;
        mpm.promptText = firePrompt;
        mpm.UpdatePrompt();
    }

    //Set targets outline to yellow. Switch back to free camera. Update prompt for locking on. 
    public void unLockToTarget()
    {
        GetComponent<CameraController>().setLookAtTarget(transform.gameObject);
        if (lockedOn)
        {
            GetComponent<CameraController>().enableFreeCam();
            lockedOn = false;
        }
        if (currentTarget != null)
        {
            currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineColor = Color.yellow;
        }
        mpm.promptText = lockOnPrompt;
        mpm.UpdatePrompt();
    }

    //Check if we have line of sight and the target appears in camera view
    void checkVisible(GameObject currentTarget)
    {
        if ((currentTarget.transform.position - transform.position).magnitude <= maxTargetDistance)
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
    }

    //Check the target still exists (Didnt leave the game or some other reason)
    void checkAlive()
    {
        if (!currentTarget.activeInHierarchy)
        {
            targetAquired = false;
            if (lockedOn)
            {
                unLockToTarget();
            }
        }
    }

    //Check the yellow outlines target is still the closest target to us
    void checkStillClosest(GameObject currentTarget)
    {
        GameObject closestEnemy = FindClosestEnemyInView();
        if (currentTarget != closestEnemy)
        {
            targetAquired = false;
            currentTargetId = 0;
            currentTarget.transform.Find("Body").GetComponent<Outline>().OutlineWidth = 0;
            mpm.DestroyPrompt();
        }
    }

    //Find and return closest enemy ship in camera view 
    GameObject FindClosestEnemyInView()
    {
        float shortestDist = float.PositiveInfinity;
        GameObject closestEnemy = null;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Ship");

        TeamData.Team myTeam = GetComponent<PlayerController>().myTeam;

        foreach (GameObject player in players)
        {
            if ((player.transform.position - transform.position).magnitude <= maxTargetDistance)
            {
                if (player.transform.GetComponent<PlayerController>().myTeam != myTeam)
                {
                    Vector3 screenPoint = myCam.gameObject.GetComponent<Camera>().WorldToViewportPoint(player.transform.position);
                    if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
                    {
                        RaycastHit hit;
                        if (Physics.Linecast(start: transform.position, end: player.transform.position, hitInfo: out hit, layerMask: layerMask))
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
        }
        return closestEnemy;
    }

    //Set the targets outline width to a visible amount
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
