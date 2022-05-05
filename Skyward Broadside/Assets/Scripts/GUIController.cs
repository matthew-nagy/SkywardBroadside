using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{

    public enum Ammo
    {
        Normal,
        Explosive,
        Special,
        None
    }
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

    public GameObject missileImage;
    public GameObject gatlingImage;
    public GameObject shockwaveImage;
    public GameObject specialInfinityImage;

    public Material shaderMat;

    readonly int ammoMinZ = 115;
    readonly int ammoMaxZ = -115;

    readonly int healthMinZ = 115;
    readonly int healthMaxZ = -120;

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

    private bool inLead;
    private bool wasLowHealth;

    private void Awake()
    {
        topPos = normalAmmoParent.transform.localPosition;
        leftPos = explosiveAmmoParent.transform.localPosition;
        rightPos = specialAmmoParent.transform.localPosition;

        healthShaderImg = healthShaderObject.GetComponent<RawImage>();
        healthShaderImg.material = new Material(healthShaderImg.material);

        explosiveShaderImg = explosiveShaderObject.GetComponent<RawImage>();
        explosiveShaderImg.material = new Material(explosiveShaderImg.material); //instantiate new material otherwise changes would also change any other instances of this material

        specialShaderImg = specialShaderObject.GetComponent<RawImage>();
        specialShaderImg.material = new Material(specialShaderImg.material);

        musicController = audioObject.GetComponent<GameMusicController>();

        inLead = false;
        wasLowHealth = false;

        sfxController = sfxObject.GetComponentInChildren<MiscSFXController>();
    }

    private void Start()
    {
        healthDial = new Dial(healthDialParent, maxHealth, Ammo.None);
    }

    // Update is called once per frame
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

    public void UpdateWeapon(int weaponId)
    {
        currentWeaponId = weaponId;
    }

    void SwitchAmmo(Ammo ammoType)
    {

        if (topDialPos.dial.ammo == ammoType)
        {
            return;
        }
        else if (leftDialPos.dial.ammo == ammoType)
        {
            StartCoroutine(RotateRight());
        }
        else
        {
            StartCoroutine(RotateLeft());
        }
    }

    IEnumerator RotateRight()
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

    IEnumerator RotateLeft()
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

    Vector3 LerpDialPos(DialPos dialPos, float proportion)
    {
        float x = Mathf.Lerp(dialPos.position.x, dialPos.targetPos.x, proportion);
        float y = Mathf.Lerp(dialPos.position.y, dialPos.targetPos.y, proportion);

        return new Vector3(x, y, 0);
    }


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

    public void UpdateGUINormalAmmo(int value)
    {

    }

    public void UpdateGUIExplosiveAmmo(int value)
    {
        Vector3 newRotation = new Vector3(0, 0, Mathf.Lerp(ammoMinZ, ammoMaxZ, (float)value / (float)maxExplosiveAmmo));

        if (explosiveAmmoDial.dialHand != null)
        {
            explosiveAmmoDial.dialHand.rotation = Quaternion.Euler(newRotation);
        }
        explosiveShaderImg.material.SetFloat("_FullProportion", (float)value / (float)maxExplosiveAmmo);

        //print("Remaining explosive: " + value);

    }

    public void UpdateGUISpecialAmmo(int value)
    {
        if (!hasGatling)
        {
            Vector3 newRotation = new Vector3(0, 0, Mathf.Lerp(ammoMinZ, ammoMaxZ, (float)value / (float)maxSpecialAmmo));
            specialAmmoDial.dialHand.rotation = Quaternion.Euler(newRotation);
            specialShaderImg.material.SetFloat("_FullProportion", (float)value / (float)maxSpecialAmmo);

        }
    }

    public void UpdateGUIScores(int myTeam, int otherTeam)
    {
        myScore.text = myTeam.ToString();
        gameOverYourTeam.text = myTeam.ToString();

        theirScore.text = otherTeam.ToString();
        gameOverOtherTeam.text = otherTeam.ToString();

        if (sfxController != null)
        {
            if (!inLead && myTeam > otherTeam)
            {
                inLead = true;
                sfxController.PlayLeadTaken();
            }
            else if (inLead && myTeam < otherTeam)
            {
                inLead = false;
                sfxController.PlayLeadLost();
            }
        }
        

        myTeamScore = myTeam;
        otherTeamScore = otherTeam;
    }

    public void SetPlayer(GameObject _player)
    {
        print("In SetPlayer");
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

    private void Initialise()
    {
        normalAmmoDial = new Dial(normalAmmoParent, maxNormalAmmo, Ammo.Normal);
        explosiveAmmoDial = new Dial(explosiveAmmoParent, maxExplosiveAmmo, Ammo.Explosive);
    }

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

    public void UpdateTimer(TimeSpan timeRemaining)
    {
        timer.text = String.Format("{0}:{1:00}", timeRemaining.Minutes, timeRemaining.Seconds);
    }

    public void GameOver()
    {
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
 