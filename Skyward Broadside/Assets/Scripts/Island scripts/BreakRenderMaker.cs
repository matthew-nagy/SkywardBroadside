using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakRenderMaker : MonoBehaviour
{
    public BreakMaster toMakeRenderer;
    // Update is called once per frame
    void Update()
    {
        toMakeRenderer.SetupPrimeRenderer();
        Destroy(this);
    }
}
