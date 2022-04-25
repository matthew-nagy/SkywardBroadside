using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToChooseTeam : MonoBehaviour
{
    public void goToTeam()
    {
        SceneManager.LoadScene("ChooseTeam");
    }
}
