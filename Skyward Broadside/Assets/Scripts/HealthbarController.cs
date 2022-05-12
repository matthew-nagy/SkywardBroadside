using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//Class that controls what each player's healthbar looks like. This is only relevant for the player who the game instance belongs to. 
public class HealthbarController : MonoBehaviourPun
{
    public LayerMask layerMask;

    // Update is called once per frame
    void Update()
    {
        //Only change the appearance of the healhbars and nametags if this is the player that owns this instance of the game
        if (photonView.IsMine) 
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Ship");

            //Loop through all players in the game to make their healthbar and nametags visible or invisible, and set them to the correct appearance
            //based on team. A player's healthbar and nametag will be visible if the player that owns the game instance has line of sight to the
            //other player.
            foreach (GameObject player in players)
            {
                PlayerPhotonHub PPH = player.transform.root.GetComponent<PlayerPhotonHub>();

                if (PPH.healthbarAndName != null)
                {
                    PlayerUI playerUIScript = player.transform.root.GetComponent<PlayerPhotonHub>().healthbarAndName.GetComponent<PlayerUI>();

                    if (playerUIScript != null)
                    {
                        //Check if the player is in the camera's viewport
                        if (playerUIScript.PlayerIsVisible())
                        {
                            //Check if the player that owns the game instance has line of sight to the player we're currently considering
                            RaycastHit hit;

                            if (Physics.Linecast(start: transform.position, end: player.transform.position, hitInfo: out hit, layerMask: layerMask))
                            {
                                if (hit.collider.gameObject == player && !playerUIScript.isDead)
                                {
                                    //Make healthbar and nametag visible
                                    playerUIScript.SetCanvasAlpha(1f);

                                    //Team of the player that owns this game instance
                                    TeamData.Team myTeam = GetComponent<PlayerController>().myTeam;

                                    //Team of the player whose healthbar and nametag we are currently considering
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
                                    //Make healthbar and nametag invisible
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
