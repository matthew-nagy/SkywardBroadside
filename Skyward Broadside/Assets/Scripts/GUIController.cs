using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//The class which controls what the in-game GUI displays. The functions in this class are called by other classes when the GUI needs to be updated. 
public class GUIController : MonoBehaviour
{
    //The possible options for the team that was previously (before the current score update) winning the game.
    public enum PrevLeader
    {
        Nobody,
        Us,
        Them
    }

    //The possible ammo types, used to label the dials. The None member is used for the health dial.
    public enum Ammo
    {
        Normal,
        Explosive,
        Special,
        None
    }

    //A container for dials. It holds the parent game object for the dial, the maximum possible value of the dial, and
    //the type of dial that it is. It also holds the game object of the dial hand, so that this can be rotated.
    public struct Dial
    {
        public Dial(GameObject dialParent, int _maxValue, Ammo ammoType)
        {
            parent = dialParent;
            dialHand = dialParent.transform.Find("Hand");

            ammo = ammoType;

            maxValue = _maxValue;

        }
        public GameObject parent;
        public Transform dialHand;

        public int maxValue;

        public Ammo ammo;
    }

    //A representation of which ammo dial is at a certain position (of which there are 3). Also holds the target position of the dial, which is set to be the
    //current dial's next position in world space after a rotation occurs.
    public struct DialPos
    {
        public DialPos(Dial _dial, Vector3 _position)
        {
            dial = _dial;
            position = _position;
            targetPos = Vector3.zero;
        }

        public Dial dial;

        public Vector3 position;
        public Vector3 targetPos { get; set; }
    }

    public GameObject healthDialParent;
    public GameObject normalAmmoParent;
    public GameObject explosiveAmmoParent;
    public GameObject specialAmmoParent;

    //Ammo symbols
    public GameObject missileImage;
    public GameObject gatlingImage;
    public GameObject shockwaveImage;
    public GameObject specialInfinityImage;

    //The material for the dial shader, which highlights in red the portion of the dial which is "remaining".
    public Material shaderMat;

    //The Z values for when an ammo dial is pointing to zero and when an ammo dial is pointing to its max value.
    readonly int ammoMinZ = 115;
    readonly int ammoMaxZ = -115;

    //The Z values for when the health dial is pointing to zero and when it's pointing to its max value.
    readonly int healthMinZ = 115;
    readonly int healthMaxZ = -120;

    //The world space positions of the three dials. 
    Vector3 topPos;
    Vector3 leftPos;
    Vector3 rightPos;

    private Dial healthDial;
    private Dial normalAmmoDial;
    private Dial explosiveAmmoDial;
    private Dial specialAmmoDial;

    private GameObject player;

    private readonly int maxHealth = 100;
    private int maxNormalAmmo = 100;
    private int maxExplosiveAmmo = 100;
    private int maxSpecialAmmo = 100;

    private DialPos topDialPos;
    private DialPos leftDialPos;
    private DialPos rightDialPos;

    private float smallDialScale = 0.9f;
    private float bigDialScale = 1.25f;

    public GameObject healthShaderObject;
    public GameObject explosiveShaderObject;
    public GameObject specialShaderObject;

    private RawImage healthShaderImg;
    private RawImage explosiveShaderImg;
    private RawImage specialShaderImg;

    private bool isMoving = false;
    private int currentWeaponId = 0;
    private bool hasGatling = false;
    private bool isInitialised = false;

    readonly float rotationTime = 0.2f; //in seconds
    readonly int numSteps = 10;

    //Score stuff
    public Text myScore;
    public Text theirScore;

    public int myTeamScore;
    public int otherTeamScore;

    public GameObject gameOverScreen;

    public Text gameOverYourTeam;
    public Text gameOverOtherTeam;

    public Text timer;


    //Music
    public GameObject audioObject;
    private GameMusicController musicController;

    public GameObject sfxObject;
    private MiscSFXController sfxController;

    private PrevLeader prevWinner;
    private bool wasLowHealth;
    private bool justLoadedIn;

    //Called when the script instance is being loaded. Used to initialise the variables for this script and set up references to other scripts.
    private void Awake()
    {
        topPos = normalAmmoParent.transform.localPosition;
        leftPos = explosiveAmmoParent.transform.localPosition;
        rightPos = specialAmmoParent.transform.localPosition;

        healthShaderImg = healthShaderObject.GetComponent<RawImage>();
        //Instantiate new material, otherwise changes would also change any other instances of this material.
        healthShaderImg.material = new Material(healthShaderImg.material);

        explosiveShaderImg = explosiveShaderObject.GetComponent<RawImage>();
        explosiveShaderImg.material = new Material(explosiveShaderImg.material); 

        specialShaderImg = specialShaderObject.GetComponent<RawImage>();
        specialShaderImg.material = new Material(specialShaderImg.material);

        musicController = audioObject.GetComponent<GameMusicController>();

        prevWinner = PrevLeader.Nobody;
        wasLowHealth = false;
        justLoadedIn = true;

        sfxController = sfxObject.GetComponentInChildren<MiscSFXController>();
    }

    // Start is called before the first frame update.
    private void Start()
    {
        healthDial = new Dial(healthDialParent, maxHealth, Ammo.None);
    }

    // Update is called once per frame
    //If the dials are not currently rotating, then rotate the dials to the correct positions (current ammo type as the biggest, top dial).
    void Update()
    {
        if (!isMoving && isInitialised)
        {
            switch (currentWeaponId)
            {
                case 0:
                    SwitchAmmo(Ammo.Normal);
                    break;
                case 1:
                    SwitchAmmo(Ammo.Explosive);
                    break;
                default:
                    SwitchAmmo(Ammo.Special);
                    break;
            }
        }
    }

    //Changes the current ammo type to be displayed.
    public void UpdateWeapon(int weaponId)
    {
        currentWeaponId = weaponId;
    }

    //Figures out how the dials should rotate so that the current ammo dial is on the top.
    //If the dials are already in the correct positions, do nothing.
    //Otherwise, make the dials rotate clockwise or anticlockwise.
    void SwitchAmmo(Ammo ammoType)
    {

        if (topDialPos.dial.ammo == ammoType)
        {
            return;
        }
        else if (leftDialPos.dial.ammo == ammoType)
        {
            StartCoroutine(RotateClockwise());
        }
        else
        {
            StartCoroutine(RotateAnticlockwise());
        }
    }

    //Rotate all the dials clockwise. This is done by setting the target position of the dial at each position, and then linearly interpolating the
    //position of the dials over time. In addition, the size of two of the dials are changed so that the big dial is always the top one.
    IEnumerator RotateClockwise()
    {
        isMoving = true;
        topDialPos.targetPos = rightDialPos.position;
        leftDialPos.targetPos = topDialPos.position;
        rightDialPos.targetPos = leftDialPos.position;

        for (int i = 1; i < numSteps + 1; i++)
        {
            float proportion = (float)i/(float)numSteps;
            float decreasingSize = Mathf.Lerp(bigDialScale, smallDialScale, proportion);
            float increasingSize = Mathf.Lerp(smallDialScale, bigDialScale, proportion);
            topDialPos.dial.parent.transform.localPosition = LerpDialPos(topDialPos, proportion);
            topDialPos.dial.parent.transform.localScale = new Vector3(decreasingSize, decreasingSize, decreasingSize);

            leftDialPos.dial.parent.transform.localPosition = LerpDialPos(leftDialPos, proportion);
            leftDialPos.dial.parent.transform.localScale = new Vector3(increasingSize, increasingSize, increasingSize);

            rightDialPos.dial.parent.transform.localPosition = LerpDialPos(rightDialPos, proportion);

            yield return new WaitForSeconds(rotationTime/(float)numSteps);
        }

        Dial tempTopDial = topDialPos.dial;
        topDialPos.dial = leftDialPos.dial;

        leftDialPos.dial = rightDialPos.dial;

        rightDialPos.dial = tempTopDial;

        isMoving = false;

    }

    //Rotate all the dials anticlockwise. This is done by setting the target position of the dial at each position, and then linearly interpolating the
    //position of the dials over time. In addition, the size of two of the dials are changed so that the big dial is always the top one.
    IEnumerator RotateAnticlockwise()
    {
        isMoving = true;

        topDialPos.targetPos = leftDialPos.position;
        leftDialPos.targetPos = rightDialPos.position;
        rightDialPos.targetPos = topDialPos.position;

        for (int i = 1; i < numSteps + 1; i++)
        {
            float proportion = (float)i / (float)numSteps;
            float decreasingSize = Mathf.Lerp(bigDialScale, smallDialScale, proportion);
            float increasingSize = Mathf.Lerp(smallDialScale, bigDialScale, proportion);
            topDialPos.dial.parent.transform.localPosition = LerpDialPos(topDialPos, proportion);
            topDialPos.dial.parent.transform.localScale = new Vector3(decreasingSize, decreasingSize, decreasingSize);
            leftDialPos.dial.parent.transform.localPosition = LerpDialPos(leftDialPos, proportion);

            rightDialPos.dial.parent.transform.localPosition = LerpDialPos(rightDialPos, proportion);
            rightDialPos.dial.parent.transform.localScale = new Vector3(increasingSize, increasingSize, increasingSize);

            yield return new WaitForSeconds(rotationTime / (float)numSteps);
        }

        Dial tempTopDial = topDialPos.dial;
        topDialPos.dial = rightDialPos.dial;
        rightDialPos.dial = leftDialPos.dial;
        leftDialPos.dial = tempTopDial;

        isMoving = false;
    }

    //Calculate what the next position of the dial should be, based on its current position, its target position and the proportion of the
    //rotation time that has passed.
    Vector3 LerpDialPos(DialPos dialPos, float proportion)
    {
        float x = Mathf.Lerp(dialPos.position.x, dialPos.targetPos.x, proportion);
        float y = Mathf.Lerp(dialPos.position.y, dialPos.targetPos.y, proportion);

        return new Vector3(x, y, 0);
    }

    //Update the health dial. This involves rotating the dial hand by the correct amount, and setting the material properties so that the shader
    //covers the correct proportion of the dial.
    //In addition, play the low health alarm if the user has less than 20 health.
    public void UpdateGUIHealth(float value)
    {
        Vector3 newRotation = new Vector3(0, 0, Mathf.Lerp(healthMinZ, healthMaxZ, (float)value / (float)maxHealth));
        healthDial.dialHand.rotation = Quaternion.Euler(newRotation);
        healthShaderImg.material.SetFloat("_FullProportion", (float)value / (float)maxHealth);
        if (value < 20f && sfxController != null)
        {
            if (!wasLowHealth)
            {
                sfxController.PlayLowHealth();
                wasLowHealth = true;
            }
        }
        else
        {
            if (wasLowHealth)
            {
                wasLowHealth = false;
            }
        }
    }

    //Update the dial for normal cannonball ammo. This now does not do anything as we decided to make normal cannonball ammo infinite.
    public void UpdateGUINormalAmmo(int value)
    {

    }

    //Update the dial for explosive cannonball ammo.
    public void UpdateGUIExplosiveAmmo(int value)
    {
        Vector3 newRotation = new Vector3(0, 0, Mathf.Lerp(ammoMinZ, ammoMaxZ, (float)value / (float)maxExplosiveAmmo));

        if (explosiveAmmoDial.dialHand != null)
        {
            explosiveAmmoDial.dialHand.rotation = Quaternion.Euler(newRotation);
        }
        explosiveShaderImg.material.SetFloat("_FullProportion", (float)value / (float)maxExplosiveAmmo);

    }

    //Update the dial for the special ammo. If the user's special weapon is the Gatling gun, there is no need to do anything, as the Gatling gun
    //has infinite ammo.
    public void UpdateGUISpecialAmmo(int value)
    {
        if (!hasGatling)
        {
            Vector3 newRotation = new Vector3(0, 0, Mathf.Lerp(ammoMinZ, ammoMaxZ, (float)value / (float)maxSpecialAmmo));
            specialAmmoDial.dialHand.rotation = Quaternion.Euler(newRotation);
            specialShaderImg.material.SetFloat("_FullProportion", (float)value / (float)maxSpecialAmmo);

        }
    }


    //Update the scores that are displayed at the top of the screen. Also, play the appropriate sound effect if the user's team has just lost or
    //taken the lead. This is done by keeping track of the team that had previously been in the lead, and playing a sound effect if this changes.
    public void UpdateGUIScores(int myTeam, int otherTeam)
    {
        myScore.text = myTeam.ToString();
        gameOverYourTeam.text = myTeam.ToString();

        theirScore.text = otherTeam.ToString();
        gameOverOtherTeam.text = otherTeam.ToString();

        if (sfxController != null && !justLoadedIn)
        {
            if ((prevWinner == PrevLeader.Nobody || prevWinner == PrevLeader.Them) && myTeam > otherTeam)
            {
                prevWinner = PrevLeader.Us;
                sfxController.PlayLeadTaken();
            }
            else if ((prevWinner == PrevLeader.Nobody || prevWinner == PrevLeader.Us) && otherTeam > myTeam)
            {
                prevWinner = PrevLeader.Them;
                sfxController.PlayLeadLost();
            }
        }
        

        myTeamScore = myTeam;
        otherTeamScore = otherTeam;
        justLoadedIn = false;
    }

    //Get information about the player's ammo types and initialise the dials. We need to know what special weapon the player has, so that we can
    //initalise the correct special ammo dial. 
    public void SetPlayer(GameObject _player)
    {
        player = _player;
        ShipArsenal shipArsenalScript = player.GetComponent<ShipArsenal>();

        maxNormalAmmo = int.MaxValue;
        maxExplosiveAmmo = shipArsenalScript.maxExplosiveCannonballAmmo;

        Initialise();
        
        if (shipArsenalScript.weapons[2])
        {
            maxSpecialAmmo = int.MaxValue;
            InitialiseGatling();
        }
        else if (shipArsenalScript.weapons[3])
        {
            maxSpecialAmmo = shipArsenalScript.maxShockwaveAmmo;
            InitialiseShockwave();
        }
        else if (shipArsenalScript.weapons[4])
        {
            maxSpecialAmmo = shipArsenalScript.maxHomingAmmo;
            InitialiseMissile();
        }


        topDialPos = new DialPos(normalAmmoDial, topPos);
        leftDialPos = new DialPos(explosiveAmmoDial, leftPos);
        rightDialPos = new DialPos(specialAmmoDial, rightPos);

        isInitialised = true;
    }

    //Initialise the ammo dials that exist for every ship type.
    private void Initialise()
    {
        normalAmmoDial = new Dial(normalAmmoParent, maxNormalAmmo, Ammo.Normal);
        explosiveAmmoDial = new Dial(explosiveAmmoParent, maxExplosiveAmmo, Ammo.Explosive);
    }

    //Initialise the special dial when the special weapon is the Gatling gun. The Gatling gun dial has no hand (because it has infinite ammo) and has an infinity symbol instead.
    //Also, enable the correct ammo symbol.
    private void InitialiseGatling()
    {
        specialAmmoDial = new Dial(specialAmmoParent, maxSpecialAmmo, Ammo.Special);
        specialAmmoDial.dialHand.gameObject.SetActive(false);
        specialInfinityImage.SetActive(true);

        gatlingImage.SetActive(true);
        shockwaveImage.SetActive(false);
        missileImage.SetActive(false);
        specialShaderObject.SetActive(false);

        hasGatling = true;
    }

    //Initialise the special dial when the special weapon is shockwave ammo. 
    private void InitialiseShockwave()
    {
        specialAmmoDial = new Dial(specialAmmoParent, maxSpecialAmmo, Ammo.Special);
        specialAmmoDial.dialHand.gameObject.SetActive(true);
        specialInfinityImage.SetActive(false);

        gatlingImage.SetActive(false);
        shockwaveImage.SetActive(true);
        missileImage.SetActive(false);
        specialShaderObject.SetActive(true);

        hasGatling = false;
    }

    //Initialise the special dial when the special weapon is homing missiles. 
    private void InitialiseMissile()
    {
        specialAmmoDial = new Dial(specialAmmoParent, maxSpecialAmmo, Ammo.Special);
        specialAmmoDial.dialHand.gameObject.SetActive(true);
        specialInfinityImage.SetActive(false);

        gatlingImage.SetActive(false);
        shockwaveImage.SetActive(false);
        missileImage.SetActive(true);
        specialShaderObject.SetActive(true);

        hasGatling = false;
    }

    //Update the timer shown on the screen by accessing the Minutes and Seconds properties of the TimeSpan.
    public void UpdateTimer(TimeSpan timeRemaining)
    {
        timer.text = String.Format("{0}:{1:00}", timeRemaining.Minutes, timeRemaining.Seconds);
    }

    //PhotonHub calls this to change the GUI when the game ends. The timer is hidden so the Game Over text can be seen properly. The battle music is disabled
    //and the game over music is enabled. In addition, the correct ending jingle is played for the player, depending on whether their team won or lost.
    public void GameOver()
    {
        timer.gameObject.SetActive(false);
        musicController.DisableBattleMusic();

        if (sfxController != null)
        {
            if (myTeamScore >= otherTeamScore)
            {
                sfxController.PlayVictorySting();
            }
            else
            {
                sfxController.PlayDefeatSting();
            }
        }
        
        musicController.EnableGameOverMusic();
    }
}
 