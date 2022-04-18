using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonballController : MonoBehaviour
{
    public GameObject owner { get; set; }
    [SerializeField]
    ParticleSystem impact;

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(impact, transform.position, Quaternion.identity);
        if (owner != collision.gameObject)
        {
            Destroy(gameObject);
        }
    }
}
