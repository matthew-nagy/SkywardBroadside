using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;

public class PlayerController : MonoBehaviourPun
{
    /*private float moveSpeed = 7.0f;
    private float turnSpeed = 50.0f;
    private float angle;
    private Vector3 velocity;
    private Vector3 angularVelocity;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    
    #region MonoBehaviour Callbacks
    
    // Start is called before the first frame update
    void Start()
    {
        CameraFollowPlayer camera = GameObject.FindObjectOfType<CameraFollowPlayer>();

        if (camera != null)
        {
            if (photonView.IsMine)
            {
                camera.player = transform;
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on Ship prefab.", this);
        }
    }

    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerManager.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, MAYBE NOT USEFUL OUTSIDE OF TUTORIAL?
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }
        
        float forwardDirection = Input.GetAxisRaw("Vertical");
        float sideDirection = Input.GetAxisRaw("Horizontal");

        velocity = transform.forward * forwardDirection * moveSpeed;
        angularVelocity = new Vector3(0, sideDirection * turnSpeed, 0);
    }

    void FixedUpdate()
    {
        transform.position += velocity * Time.deltaTime;
        transform.Rotate(angularVelocity * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.LogFormat("COLLISION with {0}", collision.gameObject.name);
        if (!photonView.IsMine)
        {
            return;
        }

        if (!collision.gameObject.name.Contains("Ball"))
        {
            return;
        }

        transform.position = new Vector3(0, 0, 0);
    }
    
    #endregion*/
}
