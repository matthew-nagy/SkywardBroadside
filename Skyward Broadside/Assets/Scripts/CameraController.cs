using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraController : MonoBehaviourPunCallbacks
{
    public GameObject shipCam;
    GameObject thisCam;
    Cinemachine.CinemachineFreeLook camera;

    private void Start()
    {
        camera = thisCam.GetComponent<Cinemachine.CinemachineFreeLook>();
        if (photonView.IsMine)
        {
            thisCam = Instantiate(shipCam);
            camera.m_Follow = transform;
            thisCam.GetComponent<Cinemachine.CinemachineFreeLook>().m_LookAt = transform;
        }
    }

    public void disableFreeCam()
    {
        camera.m_YAxis.m_InputAxisName = "";
        camera.m_XAxis.m_InputAxisName = "";
    }

    public void enableFreeCam()
    {
        camera.m_YAxis.m_InputAxisName = "Mouse Y";
        camera.m_XAxis.m_InputAxisName = "Mouse X";
    }
}
