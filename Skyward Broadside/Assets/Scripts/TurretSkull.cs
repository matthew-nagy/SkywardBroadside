using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSkull : MonoBehaviour
{
    private float heightAboveTurret = 0.5f;
    private Vector3 screenOffset = new Vector3(0f, 20f, 0f);
    private GameObject target;

    private Vector3 targetPosition;
    private bool acquiredTarget;
    private bool gotCanvas;

    public GameObject skullImg;

    private void Awake()
    {
        acquiredTarget = false;
        gotCanvas = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
    }

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
            transform.position = Camera.main.WorldToScreenPoint(targetPosition) + screenOffset;
        }
    }

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

    public bool CheckTurretIsInCameraView()
    {
        if (target != null)
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(target.transform.position);

            if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            {
                return true;
            }
        }
        return false;
    }

    public void SetVisible()
    {
        skullImg.SetActive(true);
    }

    public void SetInvisible()
    {
        skullImg.SetActive(false);
    }

    public void DeleteSkull()
    {
        Destroy(gameObject);
    }
}
