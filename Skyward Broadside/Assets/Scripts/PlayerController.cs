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

    public TeamData.Team myTeam;

    private readonly float regenSecondsPerReloads = 1f;

    public DateTime spawnTime;

    public bool resupply;

    private void Start()
    { 
        if (photonView.IsMine)
        {
            updateScript = transform.root.GetComponent<PlayerPhotonHub>().updateScript;

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
        if (photonView.IsMine)
        {
            transform.root.GetComponent<PlayerPhotonHub>().AddDeath();

            GetComponent<ShipArsenal>().respawn();

            Vector3 spawnPosition = GameManager.Instance.GetSpawnFromTeam(myTeam).transform.position;
            transform.position = spawnPosition + new Vector3(UnityEngine.Random.Range(-80, 80), 0, UnityEngine.Random.Range(-80, 80));

            spawnTime = DateTime.Now;
        }
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
            stream.SendNext((int)myTeam);
        }
        else
        {
            myTeam = (TeamData.Team)stream.ReceiveNext();
        }
    }
}
