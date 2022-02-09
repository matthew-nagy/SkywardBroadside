using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraController : MonoBehaviourPunCallbacks
{
    public GameObject shipCam;
    GameObject thisCam;

    private void Start()
    {
        if (photonView.IsMine)
        {
            thisCam = Instantiate(shipCam);
            thisCam.GetComponent<Cinemachine.CinemachineFreeLook>().m_Follow = transform;
            thisCam.GetComponent<Cinemachine.CinemachineFreeLook>().m_LookAt = transform;
        }
    }

    public void disableFreeCam()
    {
        thisCam.GetComponent<Cinemachine.CinemachineFreeLook>().m_YAxis.m_InputAxisName = "";
        thisCam.GetComponent<Cinemachine.CinemachineFreeLook>().m_XAxis.m_InputAxisName = "";
    }

    public void enableFreeCam()
    {
        thisCam.GetComponent<Cinemachine.CinemachineFreeLook>().m_YAxis.m_InputAxisName = "Mouse Y";
        thisCam.GetComponent<Cinemachine.CinemachineFreeLook>().m_XAxis.m_InputAxisName = "Mouse X";
    }
}
