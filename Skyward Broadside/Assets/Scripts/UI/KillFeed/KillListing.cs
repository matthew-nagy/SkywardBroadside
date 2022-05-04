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

    private string Truncate(string str, int maxLength)
    {
        return str.Length <= maxLength ? str : str.Substring(0, maxLength);
    }

    public void SetNames(string killer, string killed)
    {
        uitext.text = Truncate(killer, 12) + " killed " + Truncate(killed, 12);
    }
}
