using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HealthbarController : MonoBehaviourPun
{
    public LayerMask layerMask;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Ship");
            foreach (GameObject player in players)
            {
                PlayerPhotonHub PPH = player.transform.root.GetComponent<PlayerPhotonHub>();
                ShipArsenal shipArsenal = player.GetComponent<ShipArsenal>();
                if (PPH.healthbarAndName != null)
                {
                    PlayerUI playerUIScript = player.transform.root.GetComponent<PlayerPhotonHub>().healthbarAndName.GetComponent<PlayerUI>();
                    if (playerUIScript != null)
                    {
                        if (playerUIScript.PlayerIsVisible())
                        {
                            RaycastHit hit;

                            if (Physics.Linecast(start: transform.position, end: player.transform.position, hitInfo: out hit, layerMask: layerMask))
                            {
                                if (hit.collider.gameObject == player && !playerUIScript.isDead)
                                {
                                    playerUIScript.SetCanvasAlpha(1f);

                                    TeamData.Team myTeam = GetComponent<PlayerController>().myTeam;
                                    TeamData.Team theirTeam = player.transform.GetComponent<PlayerController>().myTeam;

                                    if (myTeam == theirTeam)
                                    {
                                        playerUIScript.SetFriendlyHealthbar();
                                    }
                                    else
                                    {
                                        playerUIScript.SetEnemyHealthbar();
                                    }
                                }
                                else
                                {
                                    playerUIScript.SetCanvasAlpha(0f);

                                }
                            }
                        }
                        else
                        {
                            playerUIScript.SetCanvasAlpha(0f);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("healthbarAndName is null!");
                }
            }   
        }
        
    }
}
