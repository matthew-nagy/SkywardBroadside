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

    public void disableFreeCam()
    {
        cameraObj.m_YAxis.m_InputAxisName = "";
        //cameraObj.m_XAxis.m_InputAxisName = "";
    }

    public void enableFreeCam()
    {
        cameraObj.m_YAxis.m_InputAxisName = "Mouse Y";
        cameraObj.m_XAxis.m_InputAxisName = "Mouse X";
    }
}
