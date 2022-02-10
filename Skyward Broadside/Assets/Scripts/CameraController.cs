using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraController : MonoBehaviourPunCallbacks
{
    public GameObject shipCam;
    GameObject thisCam;
    Cinemachine.CinemachineFreeLook cameraObj;

    private void Start()
    {
        cameraObj = thisCam.GetComponent<Cinemachine.CinemachineFreeLook>();
        if (photonView.IsMine)
        {
            thisCam = Instantiate(shipCam);
            cameraObj.m_Follow = transform;
            cameraObj.m_LookAt = transform;
        }
    }

    public void disableFreeCam()
    {
        cameraObj.m_YAxis.m_InputAxisName = "";
        cameraObj.m_XAxis.m_InputAxisName = "";
    }

    public void enableFreeCam()
    {
        cameraObj.m_YAxis.m_InputAxisName = "Mouse Y";
        cameraObj.m_XAxis.m_InputAxisName = "Mouse X";
    }
}
