using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseShip : MonoBehaviour
{
    public void OnClick_selectLight()
    {
        selectShip("lightShip");
        selectPrefab("lightPlayer");
    }

    public void OnClick_selectMedium()
    {
        selectShip("mediumShip");
        selectPrefab("mediumPlayer");
    }

    public void OnClick_selectHeavy()
    {
        selectShip("heavyShip");
        selectPrefab("heavyPlayer");
    }

    // Sets the player's ship
    public void selectShip(string ship)
    {
        PlayerChoices.ship = ship;
    }

    // Sets the player's prefab 
    public void selectPrefab(string ship)
    {
        PlayerChoices.playerPrefab = ship;
        SceneManager.LoadScene("Launcher");
    }
}
