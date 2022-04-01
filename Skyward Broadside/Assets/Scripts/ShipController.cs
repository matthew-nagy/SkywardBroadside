using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;


[System.Serializable]
struct RequestedControls
{
    public bool forwards;
    public bool backwards;
    public bool turnRight;
    public bool turnLeft;
    public bool up;
    public bool down;

    public void PhotonSerialize(PhotonStream stream)
    {
        stream.SendNext(forwards);
        stream.SendNext(backwards);
        stream.SendNext(turnRight);
        stream.SendNext(turnLeft);
        stream.SendNext(up);
        stream.SendNext(down);
    }

    static public RequestedControls PhotonDeserialize(PhotonStream stream)
    {
        RequestedControls controls = new RequestedControls();
        controls.forwards = (bool)stream.ReceiveNext();
        controls.backwards = (bool)stream.ReceiveNext();
        controls.turnRight = (bool)stream.ReceiveNext();
        controls.turnLeft = (bool)stream.ReceiveNext();
        controls.up = (bool)stream.ReceiveNext();
        controls.down = (bool)stream.ReceiveNext();
        return controls;
    }
}

public class ShipController : MonoBehaviourPunCallbacks, IPunObservable
{

    Rigidbody rigidBody;

    public float angularAccel = 2f;
    public float angularMass = 5f;
    public float angularFriction = 0.02f;

    [Tooltip("How much the ship weighs; controls how forces act")]
    public float mass = 5f;
    [Tooltip("The force the ship produces to drive it forwards")]
    public float engineDriveForce = 2.25f;
    [Tooltip("Force the ship can produce to break. Airflaps, for example")]
    public float flapsBreakingCoefficient = 0.00000002f;
    [Tooltip("Drag from propeler not being fast enough to push air or something")]
    public float propelerDragCoefficient = 0.055f;
    [Tooltip("Drag on the body. This must be smaller than propeler drag, as its multplied by velocity later")]
    public float bodyDragCoefficient = 0.0002f;
    [Tooltip("Proportion of reversing force compared to forwards possible force")]
    public float reversingForceProportion = 0.05f;
    [Tooltip("Limit of forwards momentum before reversing starts rather than breaking")]
    public float breakToReverseThreashold = 8f;

    RequestedControls playerInput;

    float moveSpeed = 5f;
    //Speed to move
    Vector3 velocity;
    //Matched to velocity on fixed update, then used in collision calculations
    public Vector3 velocityBeforeCollision;
    //Direction the ship is currently turning, in degrees.
    //I imagine I can increase this to do with angular momentum(?)
    Vector3 turnDirection;
    float acceleration = 1f;
    float deceleration = 1f;
    float verticalAcceleration = 4f;
    float verticalDeceleration = 4f;
    float verticalSpeed;
    float maxVerticalSpeed = 8f;
    bool isDisabled;
    float timerDisabled;
    float totalDisabledTime;

    Color teamColour;
    bool colourSet = false;

    public List<ParticleSystem> pDriveSystem;
    public List<ParticleSystem> pClockwiseJets;
    public List<ParticleSystem> pAntiClockwiseJets;
    public List<ParticleSystem> pShootUpJet;
    public List<ParticleSystem> pShootDownJet;

    GameObject cameraObject;

    private readonly float forceToDamageMultiplier = 0.1f;

    public void SetCameraObject(GameObject cam)
    {
        cameraObject = cam;
    }

    public void InformOfFire()
    {
        if (photonView.IsMine)
        {
            cameraObject.GetComponent<CameraShaker>().DoShakeEvent(CameraShakeEvent.Fire);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        playerInput = new RequestedControls();
        velocity = new Vector3(0, 0, 0);
        velocityBeforeCollision = new Vector3(0, 0, 0);
        turnDirection = new Vector3(0, 0, 0);
        verticalSpeed = 0;
        playerInput = new RequestedControls();

        teamColour = TeamData.TeamToColour(GetComponentInParent<PlayerPhotonHub>().myTeam);
    }

    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            //Despite doing this in start, sometimes it just doesn't happen
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.interpolation = RigidbodyInterpolation.Extrapolate;
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
            GetPlayerInput();
            //GetWeaponInput();
        }
        else
        {
            playerInput = new RequestedControls();
            velocityBeforeCollision = velocity;

            if (timerDisabled < totalDisabledTime)
            {
                timerDisabled += Time.deltaTime;
            }
            else
            {
                isDisabled = false;
                timerDisabled = 0f;
            }
        }

        verticalSpeed = velocity.y;
    }

    void GetPlayerInput()
    {

        playerInput = new RequestedControls();

        playerInput.forwards = SBControls.forwards.IsHeld();
        playerInput.backwards = SBControls.backwards.IsHeld();
        playerInput.turnLeft = SBControls.left.IsHeld();
        playerInput.turnRight = SBControls.right.IsHeld();

        playerInput.up = SBControls.yAxisUp.IsHeld();
        playerInput.down = SBControls.yAxisDown.IsHeld();
        
    }

    private void FixedUpdate()
    {
        HandleForce();

        Quaternion deltaRotation = Quaternion.Euler(turnDirection);
        rigidBody.MoveRotation(rigidBody.rotation * deltaRotation);

        rigidBody.MovePosition(rigidBody.position + velocity * Time.deltaTime);

        velocityBeforeCollision = velocity;

    }

    private void OnCollisionEnter(Collision collision)
    {
        //Ignore island collision
        if (collision.gameObject.layer == 7)
            return;

        float collisionMag = collision.impulse.magnitude;
        print(collision.gameObject.name);
        if (photonView.IsMine)
        {
            cameraObject.GetComponent<CameraShaker>().DoShakeEvent(CameraShakeEvent.Hit);
        }
        //Debug.LogFormat("COLLISION with {0}", collision.gameObject.name);
        if (!photonView.IsMine)
        {
            return;
        }

        if (collision.gameObject.name.Contains("ball"))
        {   
            GameObject cannonballOwner = collision.gameObject.GetComponent<CannonballController>().owner;
            if (!GameObject.ReferenceEquals(cannonballOwner, gameObject)) {
                print("I'm in pain");
                Vector3 velocityCannonball = new Vector3(collision.rigidbody.velocity.x, 0, collision.rigidbody.velocity.z);
                Vector3 finalVelocity = velocityBeforeCollision + 0.1f * velocityCannonball;
                moveSpeed = finalVelocity.magnitude;

                velocity = finalVelocity;

                //Scale collision magnitude so don't insta die on getting hit by cannonball

                collisionMag = collisionMag * 0.4f;
            }
            else
            {
                collisionMag = 0f;
            }
        }
        else if (collision.gameObject.tag == "Ship")
        {
            //print("Collision");

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
            totalDisabledTime = 1f;
            // the 10 is needed because otherwise you insta-kill each other upon contact
            collisionMag = (massA * Vector3.SqrMagnitude(finalVelocity - initialVelocity)) / 10;

        }
        else if (collision.gameObject.tag == "Wall")
        {
            Debug.Log("Wall");
            velocity = -1f * velocity;
            isDisabled = true;
            totalDisabledTime = 0.5f;
            collisionMag = 0f;
            // no damage when colliding with invisible walls, just there to avoid going out of bounds
        }
        else if (collision.gameObject.transform.parent.tag == "Terrain") //If hit terrain
        {
            print("terrain");

            if (!collision.gameObject.GetComponent<Breakable>().broken)
            {
                velocity = -0.5f * velocity;
                isDisabled = true;
                totalDisabledTime = 0.5f;
            }

            collisionMag = rigidBody.mass * 0.5f * velocityBeforeCollision.magnitude;
        }

        // Now that the ship has reacted to the collision, we can tell the player that a collision has occured, as this will impact health
        GetComponent<ShipArsenal>().doDamage(collisionMag * forceToDamageMultiplier);
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
            stream.SendNext(teamColour.r);
            stream.SendNext(teamColour.g);
            stream.SendNext(teamColour.b);
            playerInput.PhotonSerialize(stream);
        }
        else
        {
            this.velocityBeforeCollision = (Vector3)stream.ReceiveNext();
            this.velocity = (Vector3)stream.ReceiveNext();
            this.isDisabled = (bool)stream.ReceiveNext();
            this.moveSpeed = (float)stream.ReceiveNext();
            float r = (float)stream.ReceiveNext();
            float g = (float)stream.ReceiveNext();
            float b = (float)stream.ReceiveNext();
            if(!colourSet)
            {
                transform.Find("Body").gameObject.GetComponent<Renderer>().material.SetVector("_Colour", new Vector4(r, g, b, 1f));
                colourSet = true;
            }

            RequestedControls newInput = RequestedControls.PhotonDeserialize(stream);
            if(newInput.forwards != playerInput.forwards)
            {
                SetParticles(pDriveSystem, newInput.forwards);
            }
            if(newInput.turnLeft != playerInput.turnLeft)
            {
                SetParticles(pAntiClockwiseJets, newInput.turnLeft);
            }
            if(newInput.turnRight != playerInput.turnRight)
            {
                SetParticles(pClockwiseJets, newInput.turnRight);
            }
            playerInput = newInput;
        }

    }
    #endregion

    static public float GetResistiveForce(float density, float resistiveCoefficient, float area, float v_squared)
    {
        return 0.5f * density * resistiveCoefficient * area * v_squared;
    }
    static public float GetResistanceProportionAgainstVelocity(Vector3 resistiveForce, Vector3 forwardDirection)
    {
        //If its turning, you don't want all the force applied
        return Mathf.Abs(Vector3.Dot(resistiveForce.normalized, forwardDirection.normalized));
    }

    bool GoingForwards()
    {
        if(velocity.magnitude > breakToReverseThreashold && Vector3.Dot(transform.forward, velocity) > 0.0f)
        {
            return true;
        }
        return false;
    }
    void HandleForce()
    {
        Vector3 drivingForce = new Vector3(0, 0, 0);
        Vector3 breakingForce = new Vector3(0, 0, 0);
        Vector3 turningForce = new Vector3(0, 0, 0);
        //Now react to player input
        if (playerInput.forwards)
        {
            drivingForce = transform.forward * engineDriveForce;
        }
        SetParticles(pDriveSystem, playerInput.forwards);

        if (playerInput.backwards)
        {
            if (GoingForwards())
            {
                breakingForce = -1 * flapsBreakingCoefficient * velocity * velocity.magnitude;
            }
            else
            {
                drivingForce = transform.forward * engineDriveForce * -1 * reversingForceProportion;
            }
        }

        if (playerInput.turnRight)
        {
            turningForce = new Vector3(0, angularAccel, 0);
        }
        SetParticles(pClockwiseJets, playerInput.turnRight);
        if (playerInput.turnLeft)
        {
            turningForce = new Vector3(0, angularAccel * -1.0f, 0);
        }
        SetParticles(pClockwiseJets, playerInput.turnLeft);

        if (playerInput.up)
        {
            if (verticalSpeed <= maxVerticalSpeed)
            {
                verticalSpeed += verticalAcceleration * Time.deltaTime;

            }
            Debug.Log("Shit me sideways");

        }
        else if (playerInput.down)
        {
            if (verticalSpeed >= -maxVerticalSpeed)
            {
                verticalSpeed -= verticalAcceleration * Time.deltaTime;

            }

        }
        else
        {
            if (verticalSpeed < 0f)
            {
                verticalSpeed += verticalDeceleration * Time.deltaTime;
                Mathf.Clamp(verticalSpeed, -maxVerticalSpeed, 0f);
            }
            else if (verticalSpeed > 0f)
            {
                verticalSpeed -= verticalDeceleration * Time.deltaTime;
                Mathf.Clamp(verticalSpeed, 0f, maxVerticalSpeed);
            }
        }
        SetParticles(pShootUpJet, playerInput.up);
        SetParticles(pShootDownJet, playerInput.up);

        Vector3 angularAgainst = -1 * turnDirection * angularFriction;
        turnDirection += (turningForce + angularAgainst) / angularMass;

        Vector3 dragForce = -1 * bodyDragCoefficient * velocity * velocity.magnitude;



        Vector3 propellerDrag = -1 * propelerDragCoefficient * velocity;

        Vector3 longuitudinalForce = drivingForce + breakingForce + dragForce + propellerDrag;
        Vector3 acceleration = longuitudinalForce / mass;
        Vector3 preVel = new Vector3(velocity.x, velocity.y, velocity.z);
        velocity += acceleration;
        velocity.y = verticalSpeed;

    }


    void SetParticles(List<ParticleSystem> systems, bool on)
    {
        foreach(ParticleSystem ps in systems)
        {
            if (on)
            {
                ps.Play();
            }
            else
            {
                ps.Stop();
            }
        }
    }
}