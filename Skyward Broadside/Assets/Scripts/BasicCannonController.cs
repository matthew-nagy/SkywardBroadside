using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BasicCannonController : MonoBehaviour
{
    public float rotationSensitivity = 1;
    public float power = 30;

    public GameObject cannonBall;
    public Transform shotOrigin;

    public PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = PhotonView.Get(transform.parent);
    }

    // Update is called once per frame
    void Update()
    { 
        if (photonView != null && photonView.IsMine)
        {
            getInput();
        }
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
            GameObject newCannonBall = PhotonNetwork.Instantiate(this.cannonBall.name, shotOrigin.position + shotOrigin.forward * 3, shotOrigin.rotation);
            newCannonBall.GetComponent<Rigidbody>().velocity = shotOrigin.transform.forward * power;
        }
    }
}

