using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToShip : MonoBehaviour
{
    public void goToShip()
    {
        SceneManager.LoadScene("Choose Ship");
    }
}
