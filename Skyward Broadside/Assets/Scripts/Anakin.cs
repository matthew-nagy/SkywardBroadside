using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Destroys all children to this object. Used for clouds to get rid of the in-editor marker
public class Anakin : MonoBehaviour
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
