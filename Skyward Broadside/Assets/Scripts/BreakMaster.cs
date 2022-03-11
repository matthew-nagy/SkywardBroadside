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

    public float cameraShellDeleteThreshold = 0.9f;
    bool shellDeleted = false;

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
        //Debug.Log("BM now at " + destroyedOwnedBreakables + "/" + peakOwnedBreakables);

        float db = (float)destroyedOwnedBreakables;
        float max = (float)peakOwnedBreakables;
        if(db >= (max * cameraShellDeleteThreshold) && !shellDeleted)
        {
            Destroy(transform.Find("Camera collider").gameObject);
            shellDeleted = true;
        }
    }
}
