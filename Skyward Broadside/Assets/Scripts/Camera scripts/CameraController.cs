//This script manages the 2 main cameras. Free cam and lock on cam

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering.PostProcessing;
using Cinemachine;
using UnityEngine.UI; // Required when Using UI elements
using UnityEditor;

public class CameraController : MonoBehaviourPunCallbacks
{
    public GameObject shipCam;
    public GameObject lockOnCam;
    public GameObject minimapCam;
    public PostProcessProfile toonProfile;
    public CameraShaker shaker;
    GameObject thisCam;
    GameObject thisLockOnCam;
    GameObject thisMinimapCam;
    //Used in the weapons controller to figure out what cannons to enable
    public CinemachineFreeLook cameraObj;
    CinemachineVirtualCamera lockOnCameraObj;
    CinemachineVirtualCamera minimapCameraObj;

    //Static elements so the options menu can change the sensitivity before the game actually starts
    public static float sensitivity = 0.7f;
    static float sensitivityFactor = 0.8f;
    static bool sensitivityChange = false;
    public static Slider sensitivitySlider;

    float originalX;
    float originalY;

    public static void SetSensitivity()
    {
        sensitivity = sensitivitySlider.value;
        sensitivityChange = true;
    }

    bool freeCamDisabled;

    [SerializeField]
    GameObject IntroManager;

    //Create, enable, and configure appropiate cameras after intro is finished
    private void Start()
    {
        if (photonView.IsMine)
        {

            thisCam = Instantiate(shipCam);
            thisCam.name = "ShipCam";
            Blackboard.shipCamera = thisCam;
            cameraObj = thisCam.GetComponent<CinemachineFreeLook>();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            cameraObj.m_Follow = transform;
            cameraObj.m_LookAt = transform;
            cameraObj.Priority = 0;                                                                         

            IntroManager.GetComponent<Intro>().brain = thisCam.GetComponent<CinemachineBrain>();

            originalX = cameraObj.m_XAxis.m_MaxSpeed;
            originalY = cameraObj.m_YAxis.m_MaxSpeed;

            cameraObj.m_XAxis.m_MaxSpeed *= sensitivity * sensitivityFactor * 2.0f;
            cameraObj.m_YAxis.m_MaxSpeed *= sensitivity * sensitivityFactor / 1.3f;

            gameObject.GetComponent<TargetingSystem>().myCam = cameraObj;
            gameObject.GetComponent<ShipController>().freeCameraObject = cameraObj.gameObject;

            thisLockOnCam = Instantiate(lockOnCam);
            lockOnCameraObj = thisLockOnCam.GetComponent<CinemachineVirtualCamera>();
            lockOnCameraObj.m_Follow = transform;
            lockOnCameraObj.m_LookAt = transform;
            lockOnCameraObj.Priority = 0;
            gameObject.GetComponent<ShipController>().lockOnCameraObject = lockOnCameraObj.gameObject;

            thisMinimapCam = Instantiate(minimapCam, transform.position + new Vector3(0f, 500f, 0f), transform.rotation);
            thisMinimapCam.transform.Rotate(90f, 0f, 0f);
            thisMinimapCam.transform.parent = transform;

            freeCamDisabled = false;
            shaker = thisCam.GetComponent<CameraShaker>();
            shaker.freeCam = !freeCamDisabled;
        }
    }

    //Update is called once per frame.
    private void Update()
    {
        //Change the maximum speed of the camera if you changed the sensitivity
        if (sensitivityChange)
        {
            sensitivityChange = false;
            cameraObj.m_XAxis.m_MaxSpeed = originalX * ( sensitivity * sensitivityFactor * 2.0f );
            cameraObj.m_YAxis.m_MaxSpeed = originalY * ( sensitivity * sensitivityFactor / 1.3f );
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        //If lock on cam is enables, update its position and rotation to look at the target whilst keeping our ship in view
        if (photonView.IsMine)
        {
            if (freeCamDisabled)
            {
                if (PhotonView.Find(GetComponent<TargetingSystem>().currentTargetId) != null) {
                    GameObject target = PhotonView.Find(GetComponent<TargetingSystem>().currentTargetId).gameObject;
                    Vector3 targetPos = target.transform.position;
                    Vector3 v = gameObject.transform.position - targetPos;
                    Vector3 vnorm = (gameObject.transform.position - targetPos).normalized;
                    v += (vnorm * 18f);
                    v.y += 3f;
                    lockOnCameraObj.transform.position = Vector3.Lerp(lockOnCameraObj.transform.position, targetPos + v, 0.2f);
                }
                else
                {
                    enableFreeCam();
                    Debug.LogWarning("Target photonView not found. Maybe they left the game?");
                }
            }
        }
    }

    //Switch to the lockon camera and disable mouse controlled camera movement
    public void disableFreeCam()
    {
        shaker.freeCam = false;
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

    //Switch to free cam and enable mouse controller cam movement
    public void enableFreeCam()
    {
        shaker.freeCam = true;
        freeCamDisabled = false;
        cameraObj.m_XAxis.m_InputAxisName = "Mouse X";
        cameraObj.m_YAxis.m_InputAxisName = "Mouse Y";

        cameraObj.ForceCameraPosition(lockOnCameraObj.transform.position, lockOnCameraObj.transform.rotation);

        cameraObj.Priority = 1;
        lockOnCameraObj.Priority = 0;
    }

    //Set the look at transform fr the lock on camera
    public void setLookAtTarget(GameObject target)
    {
        lockOnCameraObj.m_LookAt = target.transform;
    }
}