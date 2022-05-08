using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TurretSkullController : MonoBehaviourPun
{
    public LayerMask layerMask;

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        List<GameObject> turrets = TurretsList.aliveTurrets;

        foreach (GameObject turret in turrets)
        {
            if (turret != null)
            {
                Turret turretScript = turret.GetComponent<Turret>();
                if (turretScript != null)
                {
                    if (turretScript.skullScript != null)
                    {
                        TurretSkull skullScript = turretScript.skullScript;

                        if (skullScript.CheckTurretIsInCameraView()) //check if the turret is in the camera's viewport space
                        {
                            RaycastHit hit;
                            if (Physics.Linecast(start: transform.position, end: turret.transform.position, hitInfo: out hit, layerMask: layerMask)) //check if the player can see the turret
                            {
                                if (hit.collider.gameObject == turret)
                                {
                                    skullScript.SetVisible();
                                }
                                else
                                {
                                    skullScript.SetInvisible();
                                }
                            }
                            else
                            {
                                skullScript.SetInvisible();
                            }
                        }
                        else
                        {
                            skullScript.SetInvisible();
                        }
                    }
                }
            }
        }
    }

}
