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
    [SerializeField]
    GameObject explosionMetal;
    [SerializeField]
    GameObject explosionAir;

    GameObject effect;

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
            if (collision.collider.transform.root.childCount > 0)
            {
                if (collision.collider.transform.root.GetChild(0).childCount > 0)
                {
                    if (collision.collider.transform.root.GetChild(0).GetChild(0).CompareTag("Ship"))
                    {
                        if (explosionMetal != null)
                        {
                            effect = explosionMetal;
                        }
                    }
                }
            }
            else if (collision.collider.gameObject.CompareTag("Terrain"))
            {
                if (explosionDebris != null)
                {
                    effect = explosionDebris;
                }
            }
            else
            {
                if (explosionAir != null)
                {
                    effect = explosionAir;
                }
            }
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (effect != null)
        {
            owner.transform.root.Find("SoundFxHub").GetComponent<SoundFxHub>().DoEffect(effect, transform.position);
        }
    }
}
