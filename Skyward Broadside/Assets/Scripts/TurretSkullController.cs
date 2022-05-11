using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//Class that controls whether each skull image above a turret appears or not. This is only relevant for the player who the game instance belongs to. 
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

        //Loop though all turrets that are still alive and set their skull images to be visible or invisible depending on whether the player has 
        //line of sight to the turret.
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

                        //Check if the turret is in the camera's viewport space
                        if (skullScript.CheckTurretIsInCameraView()) 
                        {
                            //Check if the player that owns the game instance has line of sight to the turret
                            RaycastHit hit;

                            if (Physics.Linecast(start: transform.position, end: turret.transform.position, hitInfo: out hit, layerMask: layerMask))
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
