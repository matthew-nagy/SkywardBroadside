using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering.PostProcessing;
using Cinemachine;

public class CameraController : MonoBehaviourPunCallbacks
{
    public GameObject shipCam;
    public GameObject lockOnCam;
    public PostProcessProfile toonProfile;
    GameObject thisCam;
    GameObject thisLockOnCam;
    //Used in the weapons controller to figure out what cannons to enable
    [Tooltip("DON'T HECCIN TOUCH THIS")]
    public CinemachineFreeLook cameraObj;
    CinemachineVirtualCamera lockOnCameraObj;

    bool freeCamDisabled;

    private void Start()
    {
        if (photonView.IsMine)
        {
            thisCam = Instantiate(shipCam);
            Blackboard.shipCamera = thisCam;
            cameraObj = thisCam.GetComponent<CinemachineFreeLook>();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            cameraObj.m_Follow = transform;
            cameraObj.m_LookAt = transform;
            cameraObj.Priority = 1;
            gameObject.GetComponent<TargetingSystem>().myCam = cameraObj;
            gameObject.GetComponent<ShipController>().SetCameraObject(cameraObj.gameObject);

            thisLockOnCam = Instantiate(lockOnCam);
            lockOnCameraObj = thisLockOnCam.GetComponent<CinemachineVirtualCamera>();
            lockOnCameraObj.m_Follow = transform;
            lockOnCameraObj.m_LookAt = transform;
            lockOnCameraObj.Priority = 0;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (photonView.IsMine)
        {
            if (freeCamDisabled)
            {
                GameObject target = PhotonView.Find(GetComponent<TargetingSystem>().currentTargetId).gameObject;
                Vector3 targetPos = target.transform.position;
                Vector3 v = gameObject.transform.position - targetPos;
                Vector3 vnorm = (gameObject.transform.position - targetPos).normalized;
                v += (vnorm * 18f);
                v.y += 3f;
                lockOnCameraObj.transform.position = Vector3.Lerp(lockOnCameraObj.transform.position, targetPos + v, 0.2f);
            }
        }
    }

    private void LateUpdate()
    {
        
    }

    public void disableFreeCam()
    {
        freeCamDisabled = true;
        cameraObj.m_XAxis.m_InputAxisName = "";
        cameraObj.m_YAxis.m_InputAxisName = "";
        cameraObj.m_XAxis.m_InputAxisValue = 0;
        cameraObj.m_YAxis.m_InputAxisValue = 0;

        lockOnCameraObj.m_Follow = null;

        float camDistFromShip = (cameraObj.transform.position - transform.position).magnitude;

        if (photonView.IsMine)
        {
            if (freeCamDisabled)
            {
                GameObject target = PhotonView.Find(GetComponent<TargetingSystem>().currentTargetId).gameObject;
                Vector3 targetPos = target.transform.position;

                float camHeightComparedToTarget = targetPos.y - cameraObj.transform.position.y;

                Vector3 v = gameObject.transform.position - targetPos;
                Vector3 vnorm = (gameObject.transform.position - targetPos).normalized;
                v += (vnorm * camDistFromShip * 0.6f);
                v.y += 3f;
                lockOnCameraObj.ForceCameraPosition(targetPos + v, transform.rotation);
            }
        }

        lockOnCameraObj.Priority = 1;
        cameraObj.Priority = 0;
    }

    public void enableFreeCam()
    {
        freeCamDisabled = false;
        cameraObj.m_XAxis.m_InputAxisName = "Mouse X";
        cameraObj.m_YAxis.m_InputAxisName = "Mouse Y";

        cameraObj.ForceCameraPosition(lockOnCameraObj.transform.position, lockOnCameraObj.transform.rotation);

        cameraObj.Priority = 1;
        lockOnCameraObj.Priority = 0;
    }

    public void setLookAtTarget(GameObject target)
    {
        lockOnCameraObj.m_LookAt = target.transform;
    }
}
