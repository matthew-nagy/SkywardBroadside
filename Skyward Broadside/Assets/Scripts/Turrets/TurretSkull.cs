using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class that controls the behaviour of the skull image above a particular turret. Each turret has its own skull image and thus its own TurretSkull instance.
public class TurretSkull : MonoBehaviour
{
    private float heightAboveTurret = 0.5f;
    private Vector3 screenOffset = new Vector3(0f, 20f, 0f);
    private GameObject target;

    private Vector3 targetPosition;
    private bool acquiredTarget;
    private bool gotCanvas;

    public GameObject skullImg;

    //Called when the script instance is being loaded. Used to initialise the variables for this script.
    private void Awake()
    {
        acquiredTarget = false;
        gotCanvas = false;
    }

    //Called once per frame.
    //Track the position of the turret and change the skull image's position based on where the turret is in the camera's viewport. This ensures
    //the skull is always displayed above the turret.
    private void Update()
    {
        if (!gotCanvas)
        {
            if (GameObject.Find("Canvas") != null)
            {
                transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
                gotCanvas = true;
            }
        }
        if (target != null && acquiredTarget)
        {
            targetPosition = target.transform.position;
            targetPosition.y += heightAboveTurret;

            if (Camera.main != null)
            {
                transform.position = Camera.main.WorldToScreenPoint(targetPosition) + screenOffset;
            }
        }
    }

    //Set the turret that the skull should be displayed above.
    public void SetTarget(Turret turret)
    {
        if (turret == null)
        {
            return;
        }
        target = turret.gameObject;
        turret.SetSkull(this);

        acquiredTarget = true;

        SetVisible();

    }

    //Check whether the skull's turret is in the camera's viewport.
    public bool CheckTurretIsInCameraView()
    {
        if (target != null)
        {
            if (Camera.main != null)
            {
                Vector3 screenPoint = Camera.main.WorldToViewportPoint(target.transform.position);

                if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
                {
                    return true;
                }
            }
            
        }
        return false;
    }

    //Sets the skull image to be visible.
    public void SetVisible()
    {
        skullImg.SetActive(true);
    }

    //Sets the skull image to be invisible. 
    public void SetInvisible()
    {
        skullImg.SetActive(false);
    }

    //Delete the skull game object (including this script). This is called by Turret.cs when the turret is destroyed. This ensures that there are no unnecessary
    //skull game objects in the game, improving performance.
    public void DeleteSkull()
    {
        Destroy(gameObject);
    }
}
