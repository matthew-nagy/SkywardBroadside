using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakMaster : MonoBehaviour
{
    int peakOwnedBreakables = 0;
    int ownedBreakables = 0;
    int destroyedOwnedBreakables = 0;
    [Tooltip("Proportion of pieces left until the entire structure shattered")]
    public float shatterThreashold = 0.1f;
    [Tooltip("Force on children once shattered as a proportion of the break threashold of the children")]
    public float shatterStrength = 1.2f;
    bool shatter = false;

    //How many updates since shattered
    int shatterCounter = 0;
    //How many updates once shattered before deletion
    int deletionPoint = 4;

    // Start is called before the first frame update
    void Start()
    {}

    // Update is called once per frame
    void Update()
    {
        if (destroyedOwnedBreakables == peakOwnedBreakables)
        {
            Debug.LogWarning("Deleted the master");
            Destroy(gameObject);
        }
    }

    public bool HasShattered()
    {
        return shatter;
    }

    public void IncrimentBreakables()
    {
        ownedBreakables += 1;
        if(ownedBreakables > peakOwnedBreakables)
        {
            peakOwnedBreakables = ownedBreakables;
        }
    }

    public void DecrimentBreakables()
    {
        ownedBreakables -= 1;
        float prop = ((float)ownedBreakables) / ((float)peakOwnedBreakables);
        if(prop <= shatterThreashold)
        {
            shatter = true;
        }
    }

    public void RegisterBreakableDestroyed()
    {
        destroyedOwnedBreakables += 1;
    }
}
