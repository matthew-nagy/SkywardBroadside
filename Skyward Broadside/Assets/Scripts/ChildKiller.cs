using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildKiller : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }
    }
}