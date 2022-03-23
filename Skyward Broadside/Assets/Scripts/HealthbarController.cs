using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HealthbarController : MonoBehaviourPun
{
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
                PlayerUI playerUIScript = player.transform.parent.GetComponent<PlayerPhotonHub>().healthbarAndName.GetComponent<PlayerUI>();
                if (playerUIScript.PlayerIsVisible())
                {
                    RaycastHit hit;

                    if (Physics.Linecast(transform.position, player.transform.position, out hit))
                    {
                        if (hit.collider.gameObject == player)
                        {
                            playerUIScript.SetCanvasAlpha(1f);
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
        
    }
}
