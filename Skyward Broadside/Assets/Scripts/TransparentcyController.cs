using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TransparentcyController : MonoBehaviour
{
    [SerializeField]
    List<GameObject> components;

    public float transparencyToSet = 0.6f;

    private void Start()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            AddTransparency();
        }
    }

    void AddTransparency()
    {
        foreach (GameObject component in components)
        {
            component.GetComponent<Renderer>().material.SetFloat("_ShaderAlpha", transparencyToSet);
        }
    }
}
