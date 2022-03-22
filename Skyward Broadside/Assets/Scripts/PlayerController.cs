using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun.UtilityScripts;
using System;
using System.Collections.Generic;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    private GuiUpdateScript updateScript;
    private int deaths;

    public int myTeam;

    // spawn positions
    private Vector3 redSpawn = new Vector3(300f, 5f, -400f);
    private Vector3 blueSpawn = new Vector3(-160f, 5f, -80f);

    private readonly float regenSecondsPerReloads = 1f;

    public DateTime spawnTime;

    public bool resupply;

    private void Start()
    { 
        if (photonView.IsMine)
        {
            updateScript = transform.root.GetComponent<PlayerPhotonHub>().updateScript;

            var properties = new Hashtable
            {
                { "deaths", deaths }
            };
            PhotonNetwork.SetPlayerCustomProperties(properties);

            spawnTime = DateTime.Now;

            UpdateHealth();
            UpdateAmmo();
            UpdateWeapon();

            Invoke(nameof(RegenInvoker), 5f);
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            UpdateHealth();
            UpdateAmmo();
            UpdateWeapon();
        }
    }

    void UpdateHealth()
    {
        if (updateScript != null)
        {
            float health = GetComponent<ShipArsenal>().health;
            if (health < 0f)
            {
                Die();
            }
            updateScript.UpdateGUIHealth(health);
        }
        else
        {
            Debug.LogWarning("Cannot update cannons: photon hubs update script is null");
        }
    }

    void UpdateAmmo()
    {
        if (updateScript != null)
        {
            ShipArsenal sa = GetComponent<ShipArsenal>();
            updateScript.UpdateGUIAmmo(sa.cannonballAmmo);
            updateScript.UpdateGUIExplosiveAmmo(sa.explosiveCannonballAmmo);
        }
        else
        {
            Debug.LogWarning("Cannot update ammo: photon hubs update script is null");
        }
    }

    void UpdateWeapon()
    {
        if (updateScript != null)
        {
            updateScript.UpdateWeapon(GetComponent<WeaponsController>().currentWeaponId);
        }
        else
        {
            Debug.LogWarning("Cannot update weapon on gui, photon hub'su update script is null");
        }
    }

    void Die()
    {
        // update player death count
        var properties = new System.Collections.Hashtable
        {
            { "deaths", ++deaths }
        };
        PhotonNetwork.SetPlayerCustomProperties(properties);

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(1, myTeam, raiseEventOptions, SendOptions.SendReliable);

        GetComponent<ShipArsenal>().respawn();

        if (myTeam == 0)
        {
            transform.position = redSpawn + new Vector3(UnityEngine.Random.Range(-80, 80), 0, UnityEngine.Random.Range(-80, 80));
        }
        else if (myTeam == 1)
        {
            transform.position = blueSpawn + new Vector3(UnityEngine.Random.Range(-80, 80), 0, UnityEngine.Random.Range(-80, 80));
        }
        spawnTime = DateTime.Now;

        Debug.Log(deaths);
    }

    public void RegenInvoker()
    {
        InvokeRepeating(nameof(ReloadShipArsenal), 0, regenSecondsPerReloads);
    }

    void ReloadShipArsenal()
    {
        if (resupply)
        {
            GetComponent<ShipArsenal>().resupply();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(myTeam);
        }
        else
        {
            myTeam = (int)stream.ReceiveNext();
        }
    }
}
