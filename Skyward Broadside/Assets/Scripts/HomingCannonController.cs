using UnityEngine;
using Photon.Pun;
using System.Collections;

public class HomingCannonController : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool weaponEnabled;
    public float reloadTime;
    public GameObject projectile;
    public Transform shotOrigin;

    bool reloading;
    bool serverShootFlag;
    bool sendShootToClient;
    bool clientShootFlag;

    string shipType;

    void Awake()
    {
        // we flag as don't destroy on load so that instance survives level synchronization, MAYBE NOT USEFUL OUTSIDE OF TUTORIAL?
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        serverShootFlag = sendShootToClient = clientShootFlag = false;
        shipType = transform.root.GetComponent<PlayerPhotonHub>().shipType;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            ServerUpdate();
        }
        else
        {
            ClientUpdate();
        }
    }

    void ServerUpdate()
    {
        GetInput();

        if (serverShootFlag)
        {
            serverShootFlag = false;
            Fire();
            GetShipTransform().GetComponent<ShipArsenal>().homingAmmo--;
            Reload();
        }
    }

    void ClientUpdate()
    {
        if (clientShootFlag)
        {
            clientShootFlag = false;
            Fire();
            GetShipTransform().GetComponent<ShipArsenal>().homingAmmo--;
            Reload();
        }
    }

    void GetInput()
    {
        if (weaponEnabled)
        {
            //attempt to fire the cannon
            if (SBControls.shoot.IsHeld() && !reloading && !serverShootFlag)
            {
                serverShootFlag = sendShootToClient = true;
            }
        }
    }
    Transform GetShipTransform()
    {
        return transform.root.Find("Ship").Find(shipType);
    }

    void SendShakeEvent()
    {
        if (photonView.IsMine)
        {
            ShipController myController = gameObject.GetComponentInParent<ShipController>();
            myController.InformOfFire();
        }
    }

    //fire the cannon
    void Fire()
    {
        SendShakeEvent();

        GameObject newProjectile = Instantiate(projectile, shotOrigin.position, shotOrigin.rotation);
        newProjectile.GetComponent<CannonballController>().owner = GetShipTransform().gameObject;

        if (!photonView.IsMine)
        {
            newProjectile.layer = 10;
        }

        Vector3 endPos = newProjectile.transform.position + (shotOrigin.forward * 5f) + (shotOrigin.up * 5f);
        StartCoroutine(InitialMovement(newProjectile, newProjectile.transform.position, endPos, 2f));
    }

    IEnumerator InitialMovement(GameObject projectile, Vector3 startPos, Vector3 endPos, float time)
    {
        float i = 0f;
        float rate = 1f / time;
        while (i < 1f)
        {
            i += Time.deltaTime * rate;
            projectile.transform.position = Vector3.Lerp(startPos, endPos, i);
            yield return null;
        }
    }

    public void Reload()
    {
        reloading = true;
        Invoke(nameof(WeaponStatusReady), reloadTime);
    }

    void WeaponStatusReady()
    {
        reloading = false;
    }

    void ServerPhotonStream(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.SendNext(sendShootToClient);
        if (sendShootToClient)
        {
            sendShootToClient = false;

            stream.SendNext(transform.rotation);
        }
    }

    void ClientPhotonStream(PhotonStream stream, PhotonMessageInfo info)
    {
        clientShootFlag = (bool)stream.ReceiveNext();
        if (clientShootFlag)
        {
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            ServerPhotonStream(stream, info);
        }
        else
        {
            ClientPhotonStream(stream, info);
        }
    }
}
