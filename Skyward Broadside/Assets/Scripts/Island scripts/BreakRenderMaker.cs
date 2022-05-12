using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The whole purpose of the render maker is to trigger a rendering setup on the first update
//This is because its unclear what order game objects will Start() in, but on the first update step we
//know that everything is already setup
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
