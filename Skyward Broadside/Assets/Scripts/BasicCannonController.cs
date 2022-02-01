using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCannonController : MonoBehaviour
{
    public float rotationSensitivity = 1;
    public float power = 30;

    public GameObject cannonBall;
    public Transform shotOrigin;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        getInput();
    }

    //inspired by https://www.youtube.com/watch?v=RnEO3MRPr5Y&ab_channel=AdamKonig
    void getInput() 
    {
        //rotate the cannon
        float rotationInput = Input.GetAxisRaw("Mouse Y");
        transform.Rotate(new Vector3(rotationInput, 0, 0));

        //fire the cannon
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject newCannonBall = Instantiate(cannonBall, shotOrigin.position, shotOrigin.rotation);
            newCannonBall.GetComponent<Rigidbody>().velocity = shotOrigin.transform.forward * power;
        }
    }
}

