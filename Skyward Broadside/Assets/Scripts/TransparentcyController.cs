using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//Used on player ships to make the ballon(s) transparent
public class TransparentcyController : MonoBehaviour
{
    [SerializeField]
    List<GameObject> components;

    public float transparencyToSet = 0.6f;

    private void Start()
    {
        //Only set them if you are the player
        if (GetComponent<PhotonView>().IsMine)
        {
            AddTransparency();
        }
    }

    //Sets the alpha in the cell shader
    void AddTransparency()
    {
        foreach (GameObject component in components)
        {
            component.GetComponent<Renderer>().material.SetFloat("_ShaderAlpha", transparencyToSet);
        }
    }
}
