using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


// Scipt that will load the scene Rooms
public class ToRooms : MonoBehaviour
{
    public void goToRooms()
    {
        SceneManager.LoadScene("Rooms");
    }
}