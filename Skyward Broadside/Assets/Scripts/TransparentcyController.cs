using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TransparentcyController : MonoBehaviour
{
    [SerializeField]
    List<GameObject> components;

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
            print("Setting t");
            component.GetComponent<Renderer>().material.SetFloat("_ShaderAlpha", 0.1f);
            print(component.GetComponent<Renderer>().material.GetFloat("_ShaderAlpha"));
        }
    }
}
