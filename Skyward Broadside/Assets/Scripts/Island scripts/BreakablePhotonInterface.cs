using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

public class BreakablePhotonInterface : MonoBehaviour
{
    public List<Breakable> children;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject obj = PhotonNetwork.Instantiate("Resources/Terrain/BreakMasterPrefab", transform.position, Quaternion.identity);
            RegisterChildrenAndDestroy(obj.GetComponent<BreakMaster>());
        }
        else
        {
            foreach (BreakMaster bm in Blackboard.breakMasters)
            {
                if (bm.IsInLocatioOf(transform))
                {
                    Debug.Log("Break master located");
                    RegisterChildrenAndDestroy(bm);
                }
            }
        }
    }

    void RegisterChildrenAndDestroy(BreakMaster owner)
    {
        foreach(Breakable child in children)
        {
            child.RegisterOwner(owner);
        }
        Destroy(this);
    }
}
