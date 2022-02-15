using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsController : MonoBehaviour
{
    public List<GameObject> leftCannons;
    bool leftEnabled = false;
    public List<GameObject> rightCannons;
    bool rightEnabled = false;

    bool weaponsHot = false;

    public bool hasEnabledWeapons()
    {
        return leftEnabled || rightEnabled;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {
        if (weaponsHot)
        {
            Cinemachine.CinemachineFreeLook camera = transform.GetComponent<CameraController>().cameraObj;
            Vector3 viewDir3D = camera.transform.forward;
            Vector2 viewDir2D = new Vector2(viewDir3D.x, viewDir3D.z);
            Vector2 shipRight2D = new Vector2(transform.right.x, transform.right.z);
            bool activateRight = Vector2.Dot(viewDir2D, shipRight2D) > 0.0;

            if (activateRight)
            {
                if (leftEnabled)
                {
                    DeactivateSide();
                    ActivateSide(rightCannons);
                }
                else if (!rightEnabled)
                {
                    ActivateSide(rightCannons);
                }
            }
            else
            {
                if (rightEnabled)
                {
                    DeactivateSide();
                    ActivateSide(leftCannons);
                }
                else if (!leftEnabled)
                {
                    ActivateSide(leftCannons);
                }
            }
        }
    }

    public void enableSideWeapons()
    {
        weaponsHot = true;
    }

    public void disableSideWeapons()
    {
        DeactivateSide();
        weaponsHot = false;
    }


    private void ActivateSide(List<GameObject> cannons)
    {
        if (cannons == leftCannons)
        {
            leftEnabled = true;
        }
        else
        {
            rightEnabled = true;
        }

        foreach (GameObject o in cannons)
        {
            o.GetComponent<BasicCannonController>().controllerActive = true;
            o.transform.rotation = transform.rotation;
            o.transform.Rotate(0, 0, 90);
        }
        //I don't think this doesn much anymore
        this.transform.GetComponent<CameraController>().disableFreeCam();
    }
    private void DeactivateSide()
    {
        List<GameObject> cannons;
        if (leftEnabled)
        {
            leftEnabled = false;
            cannons = leftCannons;
        }
        else
        {
            rightEnabled = false;
            cannons = rightCannons;
        }
        foreach (GameObject o in cannons)
        {
            o.GetComponent<BasicCannonController>().controllerActive = false;
        }

        //I don't think this doesn much anymore
        this.transform.GetComponent<CameraController>().enableFreeCam();
    }
}
