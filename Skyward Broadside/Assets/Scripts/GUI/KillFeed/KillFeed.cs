using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeed : MonoBehaviour
{
    public static KillFeed Instance;
    [SerializeField] private KillListing killListingPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Add a new listing tot he killfeed
    public void AddNewListing(string killer, string killed)
    {
        KillListing temp = Instantiate(killListingPrefab, transform);
        temp.transform.SetSiblingIndex(0);
        temp.SetNames(killer, killed);
    }
}
