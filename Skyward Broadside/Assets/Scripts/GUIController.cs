using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{
    public enum Position
    {
        Left,
        Top,
        Right,
        Health
    }

    public enum Ammo
    {
        Normal,
        Explosive,
        Special,
        None
    }
    public struct Dial
    {
        public Dial(GameObject dialParent, int _minValue, int _maxValue, Ammo ammoType)
        {
            parent = dialParent;
            dialHand = dialParent.transform.Find("Hand");

            ammo = ammoType;

            minValue = _minValue;
            maxValue = _maxValue;

        }
        public GameObject parent;
        public Transform dialHand;

        public int minValue;
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

    readonly int ammoMinZ = 115;
    readonly int ammoMaxZ = -115;

    readonly int healthMinZ = 120;
    readonly int healthMaxZ = -115;

    Vector3 topPos;
    Vector3 leftPos;
    Vector3 rightPos;

    private Dial healthDial;
    private Dial normalAmmoDial;
    private Dial explosiveAmmoDial;
    private Dial specialAmmoDial;

    private GameObject player;

    private readonly int maxHealth = 100;
    private int maxNormalAmmo;
    private int maxExplosiveAmmo;
    private int maxSpecialAmmo;

    private DialPos topDialPos;
    private DialPos leftDialPos;
    private DialPos rightDialPos;

    private Coroutine currentCoroutine = null;

    private bool isMoving = false;

    //Score stuff
    public Text myScore;
    public Text theirScore;

    public Text gameOverYourTeam;
    public Text gameOverOtherTeam;

    

    // Start is called before the first frame update
    void Start()
    {
        topPos = new Vector3(-147, 25, 0);
        leftPos = new Vector3(-204, -96, 0);
        rightPos = new Vector3(-86, -96, 0);

        healthDial = new Dial(healthDialParent, maxHealth, 0, Ammo.None);
        normalAmmoDial = new Dial(normalAmmoParent, maxNormalAmmo, 0, Ammo.Normal);
        explosiveAmmoDial = new Dial(explosiveAmmoParent, maxExplosiveAmmo, 0, Ammo.Explosive);
        specialAmmoDial = new Dial(specialAmmoParent, maxSpecialAmmo, 0, Ammo.Special);

        topDialPos = new DialPos(normalAmmoDial, topPos);
        leftDialPos = new DialPos(explosiveAmmoDial, leftPos);
        rightDialPos = new DialPos(specialAmmoDial, rightPos);

    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
        {
            currentCoroutine = StartCoroutine(RotateLeft());
        }
    }

    public void UpdateWeapon(int weaponId)
    {
        switch (weaponId)
        {
            

        }
    }

    void SwitchToNormalAmmo()
    {

        if (topDialPos.dial.ammo == Ammo.Normal)
        {
            return;
        }
        else if (topDialPos.dial.ammo == Ammo.Explosive)
        {
            
        }
        else
        {

        }
    }

    IEnumerator RotateRight()
    {
        isMoving = true;
        topDialPos.targetPos = rightDialPos.position;
        leftDialPos.targetPos = topDialPos.position;
        rightDialPos.targetPos = leftDialPos.position;

        for (int i = 1; i < 201; i++)
        {
            float proportion = (float)i/ 200f;
            float decreasingSize = Mathf.Lerp(1.0f, 0.75f, proportion);
            float increasingSize = Mathf.Lerp(0.75f, 1.0f, proportion);
            topDialPos.dial.parent.transform.localPosition = LerpDialPos(topDialPos, proportion);
            topDialPos.dial.parent.transform.localScale = new Vector3(decreasingSize, decreasingSize, decreasingSize);
            leftDialPos.dial.parent.transform.localPosition = LerpDialPos(leftDialPos, proportion);
            leftDialPos.dial.parent.transform.localScale = new Vector3(increasingSize, increasingSize, increasingSize);

            rightDialPos.dial.parent.transform.localPosition = LerpDialPos(rightDialPos, proportion);

            yield return null;
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

        for (int i = 1; i < 201; i++)
        {
            float proportion = (float)i / 200f;
            float decreasingSize = Mathf.Lerp(1.0f, 0.75f, proportion);
            float increasingSize = Mathf.Lerp(0.75f, 1.0f, proportion);
            topDialPos.dial.parent.transform.localPosition = LerpDialPos(topDialPos, proportion);
            topDialPos.dial.parent.transform.localScale = new Vector3(decreasingSize, decreasingSize, decreasingSize);
            leftDialPos.dial.parent.transform.localPosition = LerpDialPos(leftDialPos, proportion);

            rightDialPos.dial.parent.transform.localPosition = LerpDialPos(rightDialPos, proportion);
            rightDialPos.dial.parent.transform.localScale = new Vector3(increasingSize, increasingSize, increasingSize);

            yield return null;
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

    IEnumerator Move(Dial dial, Vector3 targetPos, float targetSize)
    {
        yield return null;
    }


    public void UpdateHealth(int value)
    {
        Vector3 newRotation = new Vector3(0, 0, Mathf.Lerp(healthMinZ, healthMaxZ, value / maxHealth));
        healthDial.dialHand.rotation = Quaternion.Euler(newRotation);
    }

    public void UpdateScores(int myTeam, int otherTeam)
    {
        myScore.text = myTeam.ToString();
        gameOverYourTeam.text = myTeam.ToString();

        theirScore.text = otherTeam.ToString();
        gameOverOtherTeam.text = otherTeam.ToString();
    }

    public void SetPlayer(GameObject _player)
    {
        player = _player;
        ShipArsenal shipArsenalScript = player.GetComponent<ShipArsenal>();

        maxExplosiveAmmo = shipArsenalScript.maxExplosiveCannonballAmmo;
        
    }
}
