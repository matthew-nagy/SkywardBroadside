using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCullSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<ShipController>() == null)
        {
            Destroy(collision.gameObject);
        }
        collision.gameObject.transform.position = new Vector3(collision.gameObject.transform.position.x, 3f, collision.gameObject.transform.position.y);
    }
}
