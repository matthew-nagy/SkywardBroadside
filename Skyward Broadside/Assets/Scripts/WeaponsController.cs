using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsController : MonoBehaviour
{
    public bool freeCamEnabled;
    public bool weaponCamEnabled;

    public GameObject cannon1;
    public GameObject cannon3;
    public GameObject cannon5;

    // Start is called before the first frame update
    void Start()
    {
        freeCamEnabled = true;
    }

    public void enableRightSideWeapons()
    {
        freeCamEnabled = false;
        weaponCamEnabled = true;

        cannon1.GetComponent<BasicCannonController>().controllerActive = true;
        cannon3.GetComponent<BasicCannonController>().controllerActive = true;
        cannon5.GetComponent<BasicCannonController>().controllerActive = true;

        cannon1.transform.rotation = transform.rotation;
        cannon1.transform.Rotate(0, 0, 90);
        cannon3.transform.rotation = transform.rotation;
        cannon3.transform.Rotate(0, 0, 90);
        cannon5.transform.rotation = transform.rotation;
        cannon5.transform.Rotate(0, 0, 90);

        this.transform.GetComponent<CameraController>().disableFreeCam();
    }

    public void disableRightSideWeapons()
    {
        freeCamEnabled = true;
        weaponCamEnabled = false;

        cannon1.GetComponent<BasicCannonController>().controllerActive = false;
        cannon3.GetComponent<BasicCannonController>().controllerActive = false;
        cannon5.GetComponent<BasicCannonController>().controllerActive = false;

        this.transform.GetComponent<CameraController>().enableFreeCam();
    }
}
