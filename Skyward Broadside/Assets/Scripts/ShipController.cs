using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ShipController : MonoBehaviourPunCallbacks, IPunObservable
{
    Rigidbody rigidBody;

    float moveSpeed;
    float turnSpeed = 20f;
    //float thrust;
    float topSpeed = 7f;

    float angle;
    Vector3 velocity;
    public Vector3 velocityBeforeCollision;
    Vector3 turnDirection;
    float acceleration = 1f;
    float deceleration = 1f;
    bool isDisabled;
    float timerDisabled;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            //PlayerManager.LocalPlayerInstance.controller = this;
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

        if (!isDisabled)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            turnDirection = new Vector3(0, 1, 0) * horizontalInput;

            if (Input.GetKey(KeyCode.W))
            {
                moveSpeed = moveSpeed + acceleration * Time.fixedDeltaTime;
                velocity = transform.forward * moveSpeed;


            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveSpeed = moveSpeed + (-deceleration) * Time.fixedDeltaTime;
                velocity = transform.forward * moveSpeed;

            }

            moveSpeed = Mathf.Clamp(moveSpeed, 0, topSpeed);
        }
        else
        {
            velocityBeforeCollision = velocity;

            if (timerDisabled < 3.0f)
            {
                timerDisabled += Time.deltaTime;
            }
            else
            {
                isDisabled = false;
                timerDisabled = 0f;
            }

        }

        getInput();
    }

    void getInput()
    {
        if (Input.GetKey(KeyCode.Mouse1) && transform.GetComponent<WeaponsController>().freeCamEnabled && !transform.GetComponent<WeaponsController>().weaponCamEnabled)
        {
            transform.GetComponent<WeaponsController>().enableRightSideWeapons();
        }
        else if (!Input.GetKey(KeyCode.Mouse1) && !transform.GetComponent<WeaponsController>().freeCamEnabled && transform.GetComponent<WeaponsController>().weaponCamEnabled)
        {
            transform.GetComponent<WeaponsController>().disableRightSideWeapons();
        }
    }

    private void FixedUpdate()
    {
        Quaternion deltaRotation = Quaternion.Euler(turnDirection * turnSpeed * Time.fixedDeltaTime);
        rigidBody.MoveRotation(rigidBody.rotation * deltaRotation);

        rigidBody.MovePosition(rigidBody.position + velocity * Time.deltaTime);
        velocityBeforeCollision = velocity;


    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.LogFormat("COLLISION with {0}", collision.gameObject.name);
        if (!photonView.IsMine)
        {
            return;
        }

        if (collision.gameObject.name.Contains("ball"))
        {
            
            GameObject cannonballOwner = collision.gameObject.GetComponent<CannonballController>().owner;
            print("I'm in pain");
            if (!GameObject.ReferenceEquals(cannonballOwner, gameObject)) {
                Vector3 velocityCannonball = new Vector3(collision.rigidbody.velocity.x, 0, collision.rigidbody.velocity.z);
                Vector3 finalVelocity = velocityBeforeCollision + 0.1f * velocityCannonball;
                moveSpeed = finalVelocity.magnitude;

                velocity = finalVelocity;
            }
        }


        if (collision.gameObject.tag == "Ship")
        {
            //print("Collision");
            print(collision.gameObject.name);

            Vector3 initialVelocity = velocityBeforeCollision;
            float massA = rigidBody.mass;
            Vector3 centreA = transform.position;

            Vector3 colliderInitialVelocity = collision.transform.GetComponent<ShipController>().velocityBeforeCollision;
            float massB = collision.rigidbody.mass;
            Vector3 centreB = collision.transform.position;
            print("Collider's velocity: " + colliderInitialVelocity);
            print("Collider's centre: " + centreB);
            print("Collider's mass: " + massB);


            Vector3 finalVelocity = initialVelocity - (2 * massB / (massA + massB)) * (Vector3.Dot(initialVelocity - colliderInitialVelocity, centreA - centreB) / Vector3.SqrMagnitude(centreA - centreB)) * (centreA - centreB);
            finalVelocity = 0.8f * finalVelocity;
            print("Final velocity: " + finalVelocity);
            moveSpeed = finalVelocity.magnitude;

            velocity = finalVelocity;

            isDisabled = true;
        }

    }

    #region Pun Synchronisation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(velocityBeforeCollision);
            stream.SendNext(velocity);
            stream.SendNext(isDisabled);
            stream.SendNext(moveSpeed);

        }
        else
        {
            this.velocityBeforeCollision = (Vector3)stream.ReceiveNext();
            this.velocity = (Vector3)stream.ReceiveNext();
            this.isDisabled = (bool)stream.ReceiveNext();
            this.moveSpeed = (float)stream.ReceiveNext();

        }
    }
    #endregion


}