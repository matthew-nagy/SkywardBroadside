using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToRooms : MonoBehaviour
{
    public void goToRooms()
    {
        SceneManager.LoadScene("Rooms");
    }
}