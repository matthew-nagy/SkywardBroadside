using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun.UtilityScripts;
using System;
using System.Collections.Generic;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    private GUIController updateScript;

    public TeamData.Team myTeam;

    private readonly float regenSecondsPerReloads = 1f;

    public DateTime spawnTime;

    public string playerName;
    private string lastDamagedBy;
    public int kills { get; set; }
    public int deaths { get; set; }
    public int score { get; set; }

    public bool resupply;

    [SerializeField]
    GameObject brokenShip;

    private void Start()
    { 
        if (photonView.IsMine)
        {
            updateScript = transform.root.GetComponent<PlayerPhotonHub>().updateScript;

            if (updateScript != null)
            {
                updateScript.SetPlayer(this.gameObject);
                UpdateHealth();
                UpdateAmmo();
                UpdateWeapon();
            }

            spawnTime = DateTime.Now;

            Invoke(nameof(RegenInvoker), 5f);
            
        }

        playerName = gameObject.GetComponent<PhotonView>().Owner.NickName;
        photonHub.players.Add(playerName, this);
        Scoreboard.Instance.OnNewPlayer(this);

    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            UpdateHealth();
            UpdateAmmo();
            UpdateWeapon();
            if (updateScript == null)
            {
                updateScript = transform.root.GetComponent<PlayerPhotonHub>().updateScript;
                Debug.Log("UPDATE SCRIPT NOT FOUND");

                if (updateScript != null)
                {
                    updateScript.SetPlayer(this.gameObject);
                    UpdateHealth();
                    UpdateAmmo();
                    UpdateWeapon();
                }
            }
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
            updateScript.UpdateGUINormalAmmo(sa.cannonballAmmo);
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

    public void lastHit(string name)
    {
        lastDamagedBy = name;
    }

    private void broadcastDeath()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        string[] names = {lastDamagedBy, playerName};
        PhotonNetwork.RaiseEvent((byte) photonHub.EventCode.KillEventWithNames, names, raiseEventOptions,
            SendOptions.SendReliable);
        RaiseEventOptions raiseEventOptions2 = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        PhotonNetwork.RaiseEvent((byte) photonHub.EventCode.DeathEvent, (byte)myTeam, raiseEventOptions2,
            SendOptions.SendReliable);
    }

    void Die()
    {
        if (photonView.IsMine)
        {
            broadcastDeath();
            photonView.RPC(nameof(DeathAnimation), RpcTarget.All);
            photonView.RPC(nameof(Respawn), RpcTarget.All);
        }
    }

    [PunRPC]
    void Respawn()
    {
        Invoke(nameof(MoveShipToSpawnPoint), 2.8f);
        Invoke(nameof(Activate), 3f);
    }

    void MoveShipToSpawnPoint()
    {
        GetComponent<ShipArsenal>().Respawn();

        Vector3 spawnPosition = GameManager.Instance.GetSpawnFromTeam(myTeam).transform.position;
        transform.position = spawnPosition + new Vector3(UnityEngine.Random.Range(-80, 80), 0, UnityEngine.Random.Range(-80, 80));

        spawnTime = DateTime.Now;
    }

    void Activate()
    {
        transform.root.gameObject.SetActive(true);
    }

    [PunRPC]
    void DeathAnimation()
    {
        GameObject brokenShipObj = Instantiate(brokenShip, transform.position, transform.rotation);
        brokenShipObj.GetComponent<DeathController>().Expload();
        transform.root.gameObject.SetActive(false);
    }

    public void RegenInvoker()
    {
        InvokeRepeating(nameof(ReloadShipArsenal), 0, regenSecondsPerReloads);
    }

    void ReloadShipArsenal()
    {
        if (resupply)
        {
            GetComponent<ShipArsenal>().Resupply();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            System.Object[] stats = {myTeam, kills, deaths, score};
            stream.SendNext(stats);
        }
        else
        {
            System.Object[] stats = (System.Object[]) stream.ReceiveNext();
            myTeam = (TeamData.Team) stats[0];
            kills = (int) stats[1];
            deaths = (int) stats[2];
            score = (int) stats[3];
        }
    }
}
