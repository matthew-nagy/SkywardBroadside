using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseShip : MonoBehaviour
{
    public void OnClick_selectLight()
    {
        selectShip("light");
    }

    public void OnClick_selectMedium()
    {
        selectShip("medium");
    }

    public void OnClick_selectHeavy()
    {
        selectShip("heavy");
    }

    public void selectShip(string ship)
    {
        Debug.Log("A SHIP HAS BEEN SELECTED");
        PlayerChoices.ship = ship;
        SceneManager.LoadScene("Launcher");
    }
}
