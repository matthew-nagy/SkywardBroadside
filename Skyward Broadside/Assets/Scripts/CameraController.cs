using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviourPunCallbacks
{
    public GameObject shipCam;
    public PostProcessProfile toonProfile;
    GameObject thisCam;
    //Used in the weapons controller to figure out what cannons to enable
    [Tooltip("DON'T HECCIN TOUCH THIS")]
    public Cinemachine.CinemachineFreeLook cameraObj;

    bool freeCamDisabled;

    private void Start()
    {
        if (photonView.IsMine)
        {
            thisCam = Instantiate(shipCam);
            cameraObj = thisCam.GetComponent<Cinemachine.CinemachineFreeLook>();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            cameraObj.m_Follow = transform;
            cameraObj.m_LookAt = transform;
            gameObject.GetComponent<TargetingSystem>().myCam = cameraObj;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            if (freeCamDisabled)
            {
                Vector3 targetPos = GetComponent<TargetingSystem>().currentTarget.transform.position;
                Vector3 v = gameObject.transform.position - targetPos;
                Vector3 vnorm = (gameObject.transform.position - targetPos).normalized;
                v = v + (vnorm * 15f);
                v.y = v.y + 4;
                cameraObj.transform.position = targetPos + v;
            }
        }
    }

    public void disableFreeCam()
    {
        freeCamDisabled = true;
        cameraObj.m_XAxis.m_InputAxisName = "";
        cameraObj.m_YAxis.m_InputAxisName = "";
        cameraObj.m_XAxis.m_InputAxisValue = 0;
        cameraObj.m_YAxis.m_InputAxisValue = 0;
        cameraObj.m_Follow = null;
    }

    public void enableFreeCam()
    {
        freeCamDisabled = false;
        cameraObj.m_XAxis.m_InputAxisName = "Mouse X";
        cameraObj.m_YAxis.m_InputAxisName = "Mouse Y";
    }

    public void setLookAtTarget(GameObject target)
    {
        cameraObj.m_LookAt = target.transform;
    }

    public void setFollowTarget(GameObject target)
    {
        cameraObj.m_Follow = target.transform;
    }
}
