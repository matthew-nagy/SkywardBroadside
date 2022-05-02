using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionDebris : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(DoDestroy), 2f);
    }

    void DoDestroy()
    {
        Destroy(gameObject);
    }
}
