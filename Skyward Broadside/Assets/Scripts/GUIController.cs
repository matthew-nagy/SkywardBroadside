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
    public struct Dial
    {
        public Dial(GameObject dialParent, int _minValue, int _maxValue, Position _currentPos)
        {
            parent = dialParent;
            dialHand = dialParent.transform.Find("Hand");

            minValue = _minValue;
            maxValue = _maxValue;

            currentPos = _currentPos;
        }
        public GameObject parent;
        public Transform dialHand;

        public int minValue;
        public int maxValue;

        public Position currentPos { get; set; }
    }

    public struct DialPos
    {

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
    private int maxExplosiveAmmo;
    private int maxSpecialAmmo;

    //Score stuff
    public Text myScore;
    public Text theirScore;

    public Text gameOverYourTeam;
    public Text gameOverOtherTeam;

    // Start is called before the first frame update
    void Start()
    {
        healthDial = new Dial(healthDialParent, maxHealth, 0, Position.Health);
        explosiveAmmoDial = new Dial(explosiveAmmoParent, maxExplosiveAmmo, 0, Position.Left);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateWeapon(int weaponId)
    {
        switch (weaponId)
        {
            

        }
    }

    void SwitchToNormalAmmo()
    {
        if (normalAmmoDial.currentPos == Position.Left)
        {
            RotateRight();
        }
    }

    void RotateRight()
    {

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
