using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject shipCam;

    private void Start()
    {
        Instantiate(shipCam);
    }

    public void disableFreeCam()
    {
        shipCam.GetComponent<Cinemachine.CinemachineFreeLook>().m_YAxis.m_InputAxisName = "";
        shipCam.GetComponent<Cinemachine.CinemachineFreeLook>().m_XAxis.m_InputAxisName = "";
    }

    public void enableFreeCam()
    {
        shipCam.GetComponent<Cinemachine.CinemachineFreeLook>().m_YAxis.m_InputAxisName = "Mouse Y";
        shipCam.GetComponent<Cinemachine.CinemachineFreeLook>().m_XAxis.m_InputAxisName = "Mouse X";
    }
}
