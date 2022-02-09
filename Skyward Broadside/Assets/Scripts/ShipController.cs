using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

struct RequestedControls
{
    public bool forwards;
    public bool backwards;
    public bool turnRight;
    public bool turnLeft;
}

public class ShipController : MonoBehaviourPun
{
    Rigidbody rigidBody;

    public float turnSpeed = 10f;
    public float accelerateSpeed = 5f;

    public float mass = 1f;
    public float airDensity = 0.5f;
    public float resistanceCoefficient = 0.5f;
    public float shipWidth = 2f;
    public float shipLegth = 4f;

    RequestedControls playerInput;

    //Speed to move
    Vector3 velocity;
    //Matched to velocity on fixed update, then used in collision calculations
    public Vector3 velocityBeforeCollision;
    //Direction the ship is currently turning, in degrees.
    //I imagine I can increase this to do with angular momentum(?)
    Vector3 turnDirection;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        playerInput = new RequestedControls();
        velocity = new Vector3(0, 0, 0);
        velocityBeforeCollision = new Vector3(0, 0, 0);
        turnDirection = new Vector3(0, 0, 0);
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

        GetPlayerInput();

        GetWeaponInput();


        //Now react to player input
        if (playerInput.forwards)
        {
            velocity += transform.forward * accelerateSpeed / mass;
        }
        else if (playerInput.backwards)
        {
            velocity = new Vector3(0, 0, 0);
        }

        turnDirection = new Vector3(0, 0, 0);
        if (playerInput.turnRight)
        {
            turnDirection = new Vector3(0, turnSpeed, 0);
        }
        if (playerInput.turnLeft)
        {
            turnDirection = new Vector3(0, turnSpeed * -1.0f, 0);
        }
    }

    void GetWeaponInput()
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

    void GetPlayerInput()
    {
        playerInput = new RequestedControls();

        float forwardsBackwards = Input.GetAxisRaw("Vertical");
        float turn = Input.GetAxisRaw("Horizontal");
        if (forwardsBackwards > 0.0)
        {
            playerInput.forwards = true;
        }
        else if (forwardsBackwards < 0.0)
        {
            playerInput.backwards = true;
        }

        if (turn > 0.0)
        {
            playerInput.turnRight = true;
        }
        else if (turn < 0.0)
        {
            playerInput.turnLeft = true;
        }
    }

    private void FixedUpdate()
    {
        HandleResistiveForce();

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

        if (!collision.gameObject.name.Contains("Ball"))
        {
            return;
        }


        if (collision.gameObject.tag == "Ship")
        {
            print("Collision");

            Vector3 initialVelocity = velocityBeforeCollision;
            float massA = rigidBody.mass;


            Vector3 colliderInitialVelocity = collision.transform.GetComponent<ShipController>().velocityBeforeCollision;
            float massB = collision.rigidbody.mass;


            Vector3 finalVelocity = ((massA - massB) / (massA + massB)) * initialVelocity + (2 * massB / (massA + massB)) * colliderInitialVelocity;
            print(finalVelocity);

            velocity = finalVelocity;
        }

    }

    static public float GetResistiveForce(float density, float resistiveCoefficient, float area, float v_squared)
    {
        return 0.5f * density * resistiveCoefficient * area * v_squared;
    }
    static public float GetResistanceProportionAgainstVelocity(Vector3 resistiveForce, Vector3 forwardDirection)
    {
        //If its turning, you don't want all the force applied
        return Mathf.Abs(Vector3.Dot(resistiveForce.normalized, forwardDirection.normalized));
    }

    void HandleResistiveForce()
    {
        float resistanceAngle = Vector2.Angle(Vector2.up ,new Vector2(velocity.x, velocity.y) * -1f);
        float resistanceArea = shipWidth * Mathf.Cos(resistanceAngle) + shipLegth * Mathf.Sin(resistanceAngle);
        float resistance = GetResistiveForce(airDensity, resistanceCoefficient * velocity.magnitude, resistanceArea, Mathf.Pow(velocity.magnitude, 2f));
        //Force against the ship
        Vector3 resistiveForce = resistance * -1 * velocity.normalized;

        velocity += resistiveForce / mass;
    }

}