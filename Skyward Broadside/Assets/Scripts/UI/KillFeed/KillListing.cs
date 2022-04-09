using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillListing : MonoBehaviour
{
    private float TimeBeforeKilled = 10f;
    [SerializeField] private Text uitext; // Change to add cannon image eventually
    
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, TimeBeforeKilled);
    }

    public void SetNames(string killer, string killed)
    {
        uitext.text = killer + " killed " + killed;
    }
}
