using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Turret : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform targetTransform;
    public GameObject projectile;
    public Transform shotOrigin;
    public GameObject turretHead;
    public GameObject explosionEffect;
    public float range = 75;

    public float reloadTime = 5;
    private float lastShotTime = 0;

    bool masterClientShootFlag;
    bool clientShootFlag;

    public string targetedPlayerName = "";

    Breakable myBreakable;

    [SerializeField]
    GameObject soundFxHub;
    [SerializeField]
    GameObject explosionAir;
    [SerializeField]
    GameObject missileLaunchFx;

    [SerializeField]
    GameObject skullPrefab;
    public TurretSkull skullScript;

    // Start is called before the first frame update
    void Start()
    {
        myBreakable = gameObject.GetComponent<Breakable>();
        targetedPlayerName = "";
        enabled = false;
        Invoke(nameof(SetActive), 45.0f);

        TurretsList.AddTurret(gameObject);

        if (skullPrefab != null)
        {
            GameObject skullImage = Instantiate(skullPrefab);
            //skullImage.GetComponent<TurretSkull>().SetTarget(this);
            skullImage.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
    }

    void MoveToLayer(Transform root, int layer)
    {
        if (root.gameObject.layer != 13)
        {
            root.gameObject.layer = layer;
        }
        foreach (Transform child in root)
        {
            MoveToLayer(child, layer);
        }
    }

    void SetActive()
    {
        enabled = true;
    }

    void Update()
    {
        if (myBreakable.broken) // Breakable photon interface updates this over the network
        {
            Die();
        }
        else if (photonView.IsMine)
        {
            MasterUpdate();
        }
        else
        {
            ClientUpdate();
        }
    }

    // Update is called once per frame
    void MasterUpdate()
    {
        //try to aquire target...
        GetClosestPlayerTransform();

        //if no target, do nothing
        if (targetTransform == null || targetedPlayerName == "") return;

        //Otherwise aim
        Vector3 dist_to_target = targetTransform.position - turretHead.gameObject.transform.position;
        Vector3 dir_to_target = dist_to_target.normalized;

        //Rotate to look at target - dampening controls speed of rotation
        var rotation = Quaternion.LookRotation(dist_to_target);
        turretHead.gameObject.transform.rotation = Quaternion.Slerp(turretHead.gameObject.transform.rotation, rotation, Time.deltaTime);

        //If reloaded, shoot
        if (Time.time - lastShotTime > reloadTime)
        {
            Shoot();
            masterClientShootFlag = true;
            lastShotTime = Time.time;
        }

    }

    void ClientUpdate()
    {

        if (targetedPlayerName == "") return; // No point finding transform or shootig if there is no target

        if (clientShootFlag)
        {
            Shoot();
            clientShootFlag = false;
        }

        //Otherwise use the targetedPlayerName to find the transform of the targeted player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Ship");
        foreach (GameObject player in players)
        {
            string playerName = player.transform.root.GetComponent<PlayerPhotonHub>().healthbarAndName.GetComponent<PlayerUI>().playerNameText.text;

            if (playerName == targetedPlayerName)
            {
                targetTransform = player.transform;
                break;
            }
        }
    }

    void Shoot()
    {
        if (!targetTransform) return;

        GameObject newProjectile = Instantiate(projectile, shotOrigin.position, shotOrigin.rotation);
        newProjectile.layer = 10;
        newProjectile.GetComponent<Explosive>().owner = gameObject;
        newProjectile.tag = "TurretMissile";
        newProjectile.GetComponent<Missile>().owner = gameObject;
        newProjectile.GetComponent<Missile>().rotationDampening = 2.0f;
        newProjectile.GetComponent<Missile>().explodeTimer = 5; //Make missiles explode after 4 seconds;
        newProjectile.GetComponent<Missile>().InitialiseMissile(targetTransform);
        soundFxHub.GetComponent<SoundFxHub>().DoEffect(missileLaunchFx, transform.position);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(masterClientShootFlag);
            masterClientShootFlag = false;

            stream.SendNext(targetedPlayerName);
            stream.SendNext(turretHead.transform.rotation);

        }
        else
        {
            clientShootFlag = (bool)stream.ReceiveNext();
            targetedPlayerName = (string)stream.ReceiveNext();
            turretHead.transform.rotation = (Quaternion)stream.ReceiveNext();
        }

    }

    void GetClosestPlayerTransform()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Ship");
        float shortestDistance = float.MaxValue;

        bool didSet = false;

        GameObject closestPlayer = null;
        float closestDist = -1;

        foreach (GameObject player in players)
        {
            // If player is lower than turret dont lock on to them, stops turrets aiming down
            if (player.transform.position.y < turretHead.transform.position.y) continue;

            float dist = (player.transform.position - turretHead.transform.position).magnitude;
            if (dist <= range && dist < shortestDistance)
            {
                didSet = true;
                closestPlayer = player;
                closestDist = dist;
            }
        }

        if (!didSet)
        {
            // Debug.Log("Nothing in range");
            targetedPlayerName = "";
            targetTransform = null;
        }
        else
        {
            if (!closestPlayer) return;

            targetTransform = closestPlayer.transform;
            targetedPlayerName = targetTransform.root.GetComponent<PlayerPhotonHub>().healthbarAndName.GetComponent<PlayerUI>().playerNameText.text;
            // Debug.Log("Turret locked on to: " + targetedPlayerName + " at dist " + closestDist);
        }

    }

    void Die()
    {
        skullScript.DeleteSkull();
        TurretsList.RemoveTurret(gameObject);

        var explosionObject = (GameObject)Instantiate(explosionEffect);
        explosionObject.transform.position = transform.position;
        explosionObject.GetComponent<ParticleSystem>().Play();

        soundFxHub.GetComponent<SoundFxHub>().DoEffect(explosionAir, transform.position);
        gameObject.SetActive(false);
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void SetSkull(TurretSkull script)
    {
        skullScript = script;
    }
}
