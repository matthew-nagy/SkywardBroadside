using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun.UtilityScripts;
using System;
using System.Collections.Generic;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable//, IPunInstantiateMagicCallback
{
    public GUIController updateScript;

    public TeamData.Team myTeam;

    private readonly float regenSecondsPerReloads = 0.5f;

    public DateTime spawnTime;

    public string playerName;
    private string lastDamagedBy;
    public int kills = 0;//{ get; set; }
    public int deaths = 0;//{ get; set; }
    public int score = 0;//{ get; set; }

    public bool resupply;

    [SerializeField]
    GameObject brokenShip;
    [SerializeField]
    GameObject kill_indicator_Hub;

    public bool teamSet;

    // Creates a photon instance of the players selected ship with extra data for the players nickname and team that will be set in the PhotonShipInit script on the other clients
    public static GameObject Create(Vector3 spawnPoint)
    {
        object[] data = { PhotonNetwork.NickName, PlayerChoices.team };
        GameObject player = PhotonNetwork.Instantiate(PlayerChoices.playerPrefab, spawnPoint, Quaternion.identity, 0, data);
        return player;
    }

    private void Start()
    {
        transform.root.GetComponent<PlayerPhotonHub>().SetTeam();
        teamSet = true;
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

        // get the ship's name
        playerName = gameObject.GetComponent<PhotonView>().Owner.NickName;

        // Add to the games list of players
        photonHub.players.Add(playerName, this);
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
                PlayerPhotonHub PPH = transform.root.GetComponent<PlayerPhotonHub>();
                PlayerUI UIScript = PPH.healthbarAndName.GetComponent<PlayerUI>();
                if (UIScript != null)
                {
                    UIScript.SetDead();
                }
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
            UpdateSpecialAmmo(sa);

        }
        else
        {
            Debug.LogWarning("Cannot update ammo: photon hubs update script is null");
        }
    }

    void UpdateSpecialAmmo(ShipArsenal shipArsenal) 
    {
        int ammoValue = 0;
        if (shipArsenal.weapons[2])
        {
            ammoValue = 100; //could be anything because gatling
        }
        else if (shipArsenal.weapons[3])
        {
            ammoValue = shipArsenal.shockwaveAmmo;
        }
        else
        {
            ammoValue = shipArsenal.homingAmmo;
        }
        updateScript.UpdateGUISpecialAmmo(ammoValue);
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

    // called by other scripts to set the name of the entity that last damaged this player, used for the killfeed and scoring system
    public void lastHit(string name)
    {
        lastDamagedBy = name;
    }


    // Creates two photon events:
    // 1 - Kill event with data containing the killer and the victim
    // 2 - Death event containing the team of the victim
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
        GetComponent<ShipController>().PutOutFires();
        if (photonView.IsMine)
        {
            broadcastDeath();
            GetComponent<TargetingSystem>().unLockToTarget();
            GetComponent<ShipController>().velocity = new Vector3(0f, 0f, 0f);
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

    void EnableHealthbar()
    {
        PlayerPhotonHub PPH = transform.root.GetComponent<PlayerPhotonHub>();
        PlayerUI UIScript = PPH.healthbarAndName.GetComponent<PlayerUI>();
        if (UIScript != null)
        {
            UIScript.SetAlive();
        }
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
        EnableHealthbar();
        if (kill_indicator_Hub.GetComponent<Kill_Indicator>().indicatorShown)
        {
            kill_indicator_Hub.GetComponent<Kill_Indicator>().HideIndicator();
        }
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

    // Send the ships kills, deaths and score through the PhotonView to allow the tracking of player's statistics
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            System.Object[] stats = {kills, deaths, score};
            stream.SendNext(stats);
        }
        else
        {
            System.Object[] stats = (System.Object[]) stream.ReceiveNext();
            kills = (int) stats[0];
            deaths = (int) stats[1];
            score = (int) stats[2];
        }
    }
}
