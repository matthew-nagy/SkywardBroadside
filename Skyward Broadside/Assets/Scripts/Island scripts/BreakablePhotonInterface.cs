using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

//Takes in all the children a BreakMaster would need, and then connects them to the correct break master across photon
//Either by instantiating a new BreakMaster across the network, or by finding the correct instance to add to.

//This is needed because it is essential to the determanism that breakables are added in the same order across all clients
public class BreakablePhotonInterface : MonoBehaviour
{
    bool created = false;
    public List<Breakable> children = new List<Breakable>();


    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && !created)
        {
            PhotonNetwork.Instantiate("Terrain/BreakMasterPrefab", transform.position, Quaternion.identity);
            created = true;
        }
        foreach (BreakMaster bm in Blackboard.breakMasters)
        {
            if (bm.IsInLocatioOf(transform))
            {
                //Debug.Log("Break master located");
                RegisterChildren(bm);
                Destroy(this);
            }
        }
    }

    void RegisterChildren(BreakMaster owner)
    {
        foreach(Breakable child in children)
        {
            child.RegisterOwner(owner);
        }
        CascadeSystem attatchedSystem = GetComponent<CascadeSystem>();
        owner.TriggerFinalSetup(attatchedSystem);
    }
}
