using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMover : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mov = new Vector3(0.0f, 0.0f, 0.0f);
        mov.x = mov.x + Input.GetAxisRaw("Horizontal");
        mov.z = mov.z + Input.GetAxisRaw("Vertical");

        transform.position = transform.position + transform.rotation* (mov * Time.deltaTime * 5);
    }
}
