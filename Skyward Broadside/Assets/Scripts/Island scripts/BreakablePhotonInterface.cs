using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

public class BreakablePhotonInterface : MonoBehaviour
{
    bool created = false;
    public List<Breakable> children = new List<Breakable>();

    // Start is called before the first frame update
    void Start()
    {
    }

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
        owner.TriggerFinalSetup();
    }
}
