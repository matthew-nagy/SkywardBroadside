//This script handles the reload indicator ring. It updates the fill amount of the image to create the effect seen when reloading

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadIndicator : MonoBehaviour
{
    [SerializeField]
    Image reloadCircle;

    float reloadTime;

    private void Start()
    {
        reloadCircle.fillAmount = 0f;
        reloadTime = 2f;
    }

    public void Reload()
    {
        reloadCircle.fillAmount = 1f;
        StartCoroutine(Thing());
    }

    IEnumerator Thing()
    {
        float time = reloadTime;
        while (time > 0)
        {
            reloadCircle.fillAmount = time / reloadTime;
            time -= Time.deltaTime;
            yield return null;
        }
        reloadCircle.fillAmount = 0f;
    }
}
