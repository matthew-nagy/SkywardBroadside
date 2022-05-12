//This script manages cannonballs after they are created

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

    //Update is called once per frame
    private void Update()
    {
        if (target != null)
        {
            Magnetize();
        }
    }

    //Move the cannonball towards the target
    void Magnetize()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, magnetismMultiplier * Time.deltaTime);
    }

    //handles the collision of cannonballs with other colliders
    private void OnCollisionEnter(Collision collision)
    {
        //create impact particles
        Instantiate(impact, transform.position, Quaternion.identity);

        //if the collider is not our own ship, handle the collision
        //set appropiate sound fx
        //destroy the cannonball game obj
        if (owner != collision.gameObject)
        {
            if (collision.collider.gameObject.CompareTag("Terrain") || collision.collider.gameObject.CompareTag("Debris"))
            {
                if (explosionDebris != null)
                {
                    effect = explosionDebris;
                }
            }
            else if (collision.collider.transform.root.childCount > 0)
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
                    else
                    {
                        effect = explosionAir;
                    }
                }
                else
                {
                    effect = explosionAir;
                }
            }
            else
            {
                effect = explosionAir;
            }
            Destroy(gameObject);
        }
    }

    //called when the game obj is destroyed
    private void OnDestroy()
    {
        //if the sound fx is not null play the sound fx
        if (effect != null)
        {
            owner.transform.root.Find("SoundFxHub").GetComponent<SoundFxHub>().DoEffect(effect, transform.position);
        }
    }
}
