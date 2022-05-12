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

[System.Serializable]
public struct TeamToColour
{
    public TeamData.Team team;
    public Material material;
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
    public Vector3 velocity;
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

    TeamData.Team myTeam;
    bool colourSet = false;

    public List<ParticleSystem> pDriveSystem;
    public List<ParticleSystem> pClockwiseJets;
    public List<ParticleSystem> pAntiClockwiseJets;
    public List<ParticleSystem> pShootUpJet;
    public List<ParticleSystem> pShootDownJet;

    public GameObject freeCameraObject;
    public GameObject lockOnCameraObject;

    [SerializeField]
    float globalDamageMultiplier;
    [SerializeField]
    float debrisDamageMultiplier;
    [SerializeField]
    float terrainDamageMultiplier;
    [SerializeField]
    float projectileDamageMultiplier;
    [SerializeField]
    float missileDamageMultiplier;
    [SerializeField]
    float explosionDamageMultiplier;
    [SerializeField]
    float shipDamageMultiplier;
    [SerializeField]
    float wallDamageMultiplier;
    float forceToDamageMultiplier;
    public GameObject[] balloons;

    private Vector3 lastPosition;

    public Material purpleMat;
    public Material yellowMat;

    public List<TeamToColour> teamsToColours;
    Dictionary<TeamData.Team, Material> shipMats;

    [SerializeField]
    GameObject fires;

    [SerializeField]
    GameObject introManager;

    public void ResetLastPosition()
    {
        transform.position = lastPosition;
        rigidBody.position = lastPosition;
    }

    public void InformOfFire()
    {
        if (photonView.IsMine)
        {
            freeCameraObject.GetComponent<CameraShaker>().DoShakeEvent(CameraShakeEvent.Fire);
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

        shipMats = new Dictionary<TeamData.Team, Material>();
        foreach (TeamToColour ttc in teamsToColours)
        {
            shipMats[ttc.team] = ttc.material;
        }

        myTeam = GetComponent<PlayerController>().myTeam;
        Material myMat;
        if(myTeam == TeamData.Team.Purple)
        {
            myMat = purpleMat;
        }
        else
        {
            myMat = yellowMat;
        }
        foreach (GameObject balloon in balloons)
        {
            balloon.GetComponent<Renderer>().material = myMat;
        }

        GameObject mapCenter = GameObject.Find("Center");
        Vector3 towards = mapCenter.transform.position - transform.position;
        towards.y = 0;
        transform.rotation = Quaternion.LookRotation(towards);

        if (photonView.IsMine)
        {
            Invoke(nameof(BallonSetup), 2.0f);
        }
    }

    void BallonSetup()
    {
        foreach (GameObject go in Blackboard.yellowReloadObjects)
        {
            go.GetComponent<ReloadRegister>().Setup();
        }
        foreach (GameObject go in Blackboard.purpleReloadObjects)
        {
            go.GetComponent<ReloadRegister>().Setup();
        }
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

    public void DisableMovementFor(float seconds)
    {
        isDisabled = true;
        totalDisabledTime = seconds;
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
            if (Intro.introDone)
            {
                GetPlayerInput();
            }
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

    public void StartFires()
    {
        TurnOnParticles(fires.transform);
    }

    public void PutOutFires()
    {
        TurnOffParticles(fires.transform);
    }


    void TurnOnParticles(Transform root)
    {
        if (root.TryGetComponent(out ParticleSystem ps))
        {
            if (!ps.isPlaying)
            {
                ps.Play();
            }
        }
        foreach (Transform child in root)
        {
            TurnOnParticles(child);
        }
    }

    void TurnOffParticles(Transform root)
    {
        if (root.TryGetComponent(out ParticleSystem ps))
        {
            if (ps.isPlaying)
            {
                ps.Stop();
            }
        }
        foreach (Transform child in root)
        {
            TurnOffParticles(child);
        }
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
        lastPosition = transform.position;

        HandleForce();

        Quaternion deltaRotation = Quaternion.Euler(turnDirection);
        rigidBody.MoveRotation(rigidBody.rotation * deltaRotation);

        rigidBody.MovePosition(rigidBody.position + velocity * Time.deltaTime);

        velocityBeforeCollision = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Ignore "island" collision (not actually islands)
        if (collision.gameObject.layer == 7)
            return;


        if (photonView.IsMine)
        {
            freeCameraObject.GetComponent<CameraShaker>().DoShakeEvent(CameraShakeEvent.Hit);
            freeCameraObject.GetComponent<CameraShaker>().DoShakeEvent(CameraShakeEvent.Hit);
        }

        if (!photonView.IsMine)
        {
            return;
        }

        bool shouldDealDamage = false;

        if (collision.gameObject.name.Contains("ball"))
        {
            shouldDealDamage = true;
            GameObject cannonballOwner = collision.gameObject.GetComponent<CannonballController>().owner;
            gameObject.GetComponent<PlayerController>().lastHit(cannonballOwner.GetComponent<PhotonView>().Owner.NickName);
            if (!GameObject.ReferenceEquals(cannonballOwner, gameObject))
            {
                Vector3 velocityCannonball = new Vector3(collision.rigidbody.velocity.x, 0, collision.rigidbody.velocity.z);
                Vector3 finalVelocity = velocityBeforeCollision + 0.1f * velocityCannonball;
                moveSpeed = finalVelocity.magnitude;

                velocity = finalVelocity;

                forceToDamageMultiplier = projectileDamageMultiplier * collision.collider.GetComponent<Rigidbody>().velocity.magnitude;

                //explosive cannonballs do extra damage
                if (collision.gameObject.CompareTag("ExplosiveCannonball"))
                {
                    forceToDamageMultiplier *= explosionDamageMultiplier;
                }
            }
            else
            {
                forceToDamageMultiplier = 0f;
            }
        }
        else if (collision.gameObject.CompareTag("TurretMissile"))
        {

            Debug.Log("Missile collision");
            shouldDealDamage = true;
            gameObject.GetComponent<PlayerController>().lastHit("Turret");
            GameObject owner = collision.gameObject.GetComponent<Missile>().owner;

            if (!GameObject.ReferenceEquals(owner, gameObject))
            {
                Vector3 velocityMissile = new Vector3(collision.rigidbody.velocity.x, 0, collision.rigidbody.velocity.z);
                Vector3 finalVelocity = velocityBeforeCollision + 0.1f * velocityMissile;
                moveSpeed = finalVelocity.magnitude;

                velocity = finalVelocity;

                forceToDamageMultiplier = missileDamageMultiplier * collision.gameObject.GetComponent<Missile>().damageAmount;
            }
            else
            {
                forceToDamageMultiplier = 0f;
            }
        }
        else if (collision.gameObject.CompareTag("Missile"))
        {
            shouldDealDamage = true;
            GameObject owner = collision.gameObject.GetComponent<Missile>().owner;
            gameObject.GetComponent<PlayerController>().lastHit(owner.GetComponent<PhotonView>().Owner.NickName);
            if (!GameObject.ReferenceEquals(owner, gameObject))
            {
                Vector3 velocityMissile = new Vector3(collision.rigidbody.velocity.x, 0, collision.rigidbody.velocity.z);
                Vector3 finalVelocity = velocityBeforeCollision + 0.1f * velocityMissile;
                moveSpeed = finalVelocity.magnitude;

                velocity = finalVelocity;

                forceToDamageMultiplier = missileDamageMultiplier * collision.gameObject.GetComponent<Missile>().damageAmount;
            }
            else
            {
                forceToDamageMultiplier = 0f;
            }
        }
        else if (collision.gameObject.CompareTag("Ship"))
        {
            shouldDealDamage = true;
            gameObject.GetComponent<PlayerController>().lastHit(collision.gameObject.GetComponent<PhotonView>().Owner.NickName);

            Vector3 initialVelocity = velocityBeforeCollision;
            float massA = rigidBody.mass;
            Vector3 centreA = transform.position;
            
            //Get the mass and the velocity of the other ship before the collision. Required to calculate the velocity of this ship after the collision.
            Vector3 colliderInitialVelocity = collision.transform.GetComponent<ShipController>().velocityBeforeCollision;
            float massB = collision.rigidbody.mass;
            Vector3 centreB = collision.transform.position;

            //Use the angle-free form for an elastic oblique collision to calculate what the velocity of the ship should be after the collision.
            Vector3 finalVelocity = initialVelocity - (2 * massB / (massA + massB)) * (Vector3.Dot(initialVelocity - colliderInitialVelocity, centreA - centreB) / Vector3.SqrMagnitude(centreA - centreB)) * (centreA - centreB);
            finalVelocity = 0.8f * finalVelocity;
            moveSpeed = finalVelocity.magnitude;

            velocity = finalVelocity;

            DisableMovementFor(1f);
            // the 10 is needed because otherwise you insta-kill each other upon contact
            forceToDamageMultiplier = shipDamageMultiplier * (massA * Vector3.SqrMagnitude(finalVelocity - initialVelocity)) / 10;
        }
        //else if (collision.gameObject.tag == "Wall")
        //{
        //    shouldDealDamage = true;
        //    Debug.Log("Wall");
        //    switch (collision.gameObject.name)
        //    {
        //        case "InvisWallX+":
        //            transform.position -= new Vector3(1, 0, 0);
        //            break;
        //        case "InvisWallX-":
        //            transform.position += new Vector3(1, 0, 0);
        //            break;
        //        case "InvisWallY+":
        //            transform.position -= new Vector3(0, 1, 0);
        //            break;
        //        case "InvisWallY-":
        //            transform.position += new Vector3(0, 1, 0);
        //            break;
        //        case "InvisWallZ+":
        //           transform.position -= new Vector3(0, 0, 1);
        //            break;
        //        case "InvisWallZ-":
        //            transform.position += new Vector3(0, 0, 1);
        //            break;
        //   }
        //    velocity = new Vector3(0, 0, 0); 
        //    DisableMovementFor(0.5f);
        //    forceToDamageMultiplier = wallDamageMultiplier;
        //}
        else if (collision.gameObject.CompareTag("Debris"))
        {
            shouldDealDamage = true;
            gameObject.GetComponent<PlayerController>().lastHit("Debris");
            forceToDamageMultiplier = debrisDamageMultiplier * collision.collider.GetComponent<Rigidbody>().velocity.magnitude;
        }
        else
        {
            Transform colParent = collision.gameObject.transform.parent;
            if (colParent != null)
            {
                if (colParent.CompareTag("Terrain")) //If hit terrain
                {
                    shouldDealDamage = true;
                    gameObject.GetComponent<PlayerController>().lastHit("Terrain");

                    if (!collision.gameObject.GetComponent<Breakable>().broken)
                    {
                        //Ship moves backwards with half of its original velocity
                        velocity = -0.5f * velocity;
                        DisableMovementFor(0.5f);
                    }

                    forceToDamageMultiplier = terrainDamageMultiplier * velocity.magnitude;
                }
            }
        }

        if (shouldDealDamage)
        {
            // Now that the ship has reacted to the collision, we can tell the player that a collision has occured, as this will impact health
            GetComponent<ShipArsenal>().doDamage(globalDamageMultiplier * forceToDamageMultiplier);
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
            stream.SendNext((byte)myTeam);
            stream.SendNext(transform.position.x);
            stream.SendNext(transform.position.y);
            stream.SendNext(transform.position.z);
            var ea = transform.rotation.eulerAngles;
            stream.SendNext(ea.x);
            stream.SendNext(ea.y);
            stream.SendNext(ea.z);
            playerInput.PhotonSerialize(stream);
        }
        else
        {
            this.velocityBeforeCollision = (Vector3)stream.ReceiveNext();
            this.velocity = (Vector3)stream.ReceiveNext();
            this.isDisabled = (bool)stream.ReceiveNext();
            this.moveSpeed = (float)stream.ReceiveNext();

            myTeam = (TeamData.Team)((byte)stream.ReceiveNext());

            Vector3 np = Vector3.zero;
            Vector3 ea = Vector3.zero;
            np.x = (float)stream.ReceiveNext();
            np.y = (float)stream.ReceiveNext();
            np.z = (float)stream.ReceiveNext();
            ea.x = (float)stream.ReceiveNext();
            ea.y = (float)stream.ReceiveNext();
            ea.z = (float)stream.ReceiveNext();
            transform.position = np;
            transform.rotation = Quaternion.Euler(ea);

            if (!colourSet)
            {
                Material myMat;
                if (myTeam == TeamData.Team.Purple)
                {
                    myMat = purpleMat;
                }
                else
                {
                    myMat = yellowMat;
                }
                foreach (GameObject balloon in balloons)
                {
                    balloon.GetComponent<Renderer>().material = myMat;
                }
                colourSet = true;
            }

            RequestedControls newInput = RequestedControls.PhotonDeserialize(stream);
            if (newInput.forwards != playerInput.forwards)
            {
                SetParticles(pDriveSystem, newInput.forwards);
            }
            if (newInput.turnLeft != playerInput.turnLeft)
            {
                SetParticles(pAntiClockwiseJets, newInput.turnLeft);
            }
            if (newInput.turnRight != playerInput.turnRight)
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
        if (velocity.magnitude > breakToReverseThreashold && Vector3.Dot(transform.forward, velocity) > 0.0f)
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
        SetParticles(pAntiClockwiseJets, playerInput.turnLeft);

        if (playerInput.up)
        {
            //If player is below the max vertical speed, accelerate with constant acceleration.
            if (verticalSpeed <= maxVerticalSpeed)
            {
                verticalSpeed += verticalAcceleration * Time.deltaTime;

            }
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
            //If the player stops pressing R or F, the ship decelerates until the speed is 0.
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
        foreach (ParticleSystem ps in systems)
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