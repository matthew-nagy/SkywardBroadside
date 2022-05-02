using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonballController : MonoBehaviour
{
    public GameObject owner { get; set; }

    [SerializeField]
    ParticleSystem impact;

    float magnetismMultiplier = 100;
    public GameObject target;

    [SerializeField]
    GameObject explosionDebris;

    private void Update()
    {
        if (target != null)
        {
            Magnetize();
        }
    }

    void Magnetize()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, magnetismMultiplier * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(impact, transform.position, Quaternion.identity);
        if (owner != collision.gameObject)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        owner.transform.root.Find("SoundFxHub").GetComponent<SoundFxHub>().DoEffect(explosionDebris, transform.position);
    }
}
