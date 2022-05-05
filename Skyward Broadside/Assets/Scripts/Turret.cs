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

    public float reloadTime = 5;
    private float lastShotTime = 0;

    bool masterClientShootFlag;
    bool clientShootFlag;

    public Text targetedPlayerName;

    Breakable myBreakable;


    // Start is called before the first frame update
    void Start()
    {
        myBreakable = gameObject.GetComponent<Breakable>();
        enabled = false;
        Invoke(nameof(SetActive), 50.0f);
    }

    void SetActive()
    {
        enabled = true;
    }

    void Update()
    {
        if (myBreakable.broken)
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
        if (targetTransform == null) return;

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
        if (clientShootFlag)
        {
            Shoot();
            clientShootFlag = false;
        }

        //Otherwise use the targetedPlayerName to find the transform of the targeted player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Ship");
        foreach (GameObject player in players)
        {
            Text playerName = targetTransform.root.GetComponent<PlayerPhotonHub>().healthbarAndName.GetComponent<PlayerUI>().playerNameText;
            if (playerName = targetedPlayerName)
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
        newProjectile.GetComponent<Missile>().InitialiseMissile(targetTransform);
        newProjectile.GetComponent<Missile>().owner = gameObject;
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
            targetedPlayerName = (Text)stream.ReceiveNext();
            turretHead.transform.rotation = (Quaternion)stream.ReceiveNext();
        }

    }

    void GetClosestPlayerTransform()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Ship");
        float shortestDistance = float.MaxValue;
        foreach (GameObject player in players)
        {
            float dist = (player.transform.position - turretHead.transform.position).magnitude;
            if (dist < shortestDistance)
            {
                targetTransform = player.transform;
            }
        }

        targetedPlayerName = targetTransform.root.GetComponent<PlayerPhotonHub>().healthbarAndName.GetComponent<PlayerUI>().playerNameText;
    }

    void Die()
    {
        var explosionObject = (GameObject)Instantiate(explosionEffect);
        explosionObject.transform.position = transform.position;
        explosionObject.GetComponent<ParticleSystem>().Play();

        gameObject.SetActive(false);
        gameObject.GetComponent<Renderer>().enabled = false;
        // gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }
}
