using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathController : MonoBehaviour
{
    [SerializeField]
    GameObject[] bodyParts;
    [SerializeField]
    GameObject[] balloonParts;
    [SerializeField]
    float explosionForce;
    [SerializeField]
    float explosionRadius;
    [SerializeField]
    ParticleSystem explosion1;

    [SerializeField]
    GameObject soundFxHub;
    [SerializeField]
    GameObject shipGoingDownFx;
    [SerializeField]
    GameObject explosionFx;

    public void Expload()
    {
        DoParticles();

        Invoke(nameof(DoSoundFx), 1f);

        foreach (GameObject part in bodyParts)
        {
            part.GetComponent<Debris>().isActive = true;
            Rigidbody rb = part.GetComponent<Rigidbody>();
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }

        foreach (GameObject part in balloonParts)
        {
            part.GetComponent<Debris>().isActive = true;
            Rigidbody rb = part.GetComponent<Rigidbody>();
            Vector3 randomness = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            rb.AddForce((transform.up.normalized + randomness.normalized) * 100f, ForceMode.Force);
            StartCoroutine(Shrink(part));
        }
    }

    IEnumerator Shrink(GameObject part)
    {
        float time = 0;
        while (time < 2f && part.transform.localScale.x >= 0f)
        {
            part.transform.localScale -= new Vector3(1f, 1f, 1f);
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(part);
    }

    void DoParticles()
    {
        StartCoroutine(DoExplosionEffect(transform.position + new Vector3(1f, 1f, 0f), 0f));
        StartCoroutine(DoExplosionEffect(transform.position + new Vector3(0f, -1f, 1f), 0.2f));
        StartCoroutine(DoExplosionEffect(transform.position + new Vector3(-1f, 0f, -1f), 0.4f));
    }

    IEnumerator DoExplosionEffect(Vector3 pos, float delay)
    { 
        yield return new WaitForSeconds(delay);
        Instantiate(explosion1, pos, Quaternion.identity);
        soundFxHub.GetComponent<SoundFxHub>().DoEffect(explosionFx, transform.position);
        yield return null;
    }

    void DoSoundFx()
    {
        soundFxHub.GetComponent<SoundFxHub>().DoEffect(shipGoingDownFx, transform.position);
    }
}
