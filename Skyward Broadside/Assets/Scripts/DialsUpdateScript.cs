using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialsUpdateScript : MonoBehaviour
{
    public struct Dial
    {
        public Dial(GameObject dialParent, int _minValue, int _maxValue)
        {
            parent = dialParent;
            dialHand = dialParent.transform.Find("Hand");

            minValue = _minValue;
            maxValue = _maxValue;
        }
        public GameObject parent;
        public Transform dialHand;

        public int minValue;
        public int maxValue;

    }

    public GameObject healthDialParent;

    public GameObject normalAmmoParent;

    public GameObject explosiveAmmoParent;

    public GameObject specialAmmoParent;

    int ammoMinZ = 115;
    int ammoMaxZ = -115;

    int healthMinZ = 120;
    int healthMaxZ = 115;

    private Dial healthDial;
    private Dial normalAmmoDial;
    private Dial explosiveAmmoDial;
    private Dial specialAmmoDial;

    private GameObject player;

    private readonly int maxHealth = 100;
    private int maxExplosiveAmmo;
    private int maxSpecialAmmo;

    // Start is called before the first frame update
    void Start()
    {
        ShipArsenal shipArsenalScript = player.GetComponent<ShipArsenal>();
        healthDial = new Dial(healthDialParent, maxHealth, 0);
        explosiveAmmoDial = new Dial(explosiveAmmoParent, maxExplosiveAmmo, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void UpdateHealth(int value)
    {
        Vector3 newRotation = new Vector3(0, 0, Mathf.Lerp(healthMinZ, healthMaxZ, value / maxHealth));
        healthDial.dialHand.rotation = Quaternion.Euler(newRotation);
    }

    public void SetPlayer(GameObject _player)
    {
        player = _player;
    }
}
