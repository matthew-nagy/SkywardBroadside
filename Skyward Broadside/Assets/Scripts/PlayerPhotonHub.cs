using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhotonHub : MonoBehaviour
{
    // THIS IS PUBLIC FOR NOW, TO ALLOW FINE TUNING DURING TESTING EASIER.
    public float forceToDamageMultiplier;

    private float currHealth;

    //The actual ship of the player
    private GameObject PlayerShip;
    private GuiUpdateScript updateScript;

    // Start is called before the first frame update
    void Start()
    {
        PlayerShip = this.gameObject.transform.GetChild(0).gameObject;
        updateScript = GameObject.Find("User GUI").GetComponent<GuiUpdateScript>();

        // We would want a way of accessing the players ship, and fetching the max health of only that. Probably could do it with an enum or something
        currHealth = PlayerShip.GetComponent<ShipArsenal>().maxHealth;
        updateScript.UpdateGUIHealth(currHealth);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHealth(float collisionMagnitude)
    {
        float healthVal = collisionMagnitude * forceToDamageMultiplier;
        currHealth -= healthVal;
        print(currHealth);
        updateScript.UpdateGUIHealth(currHealth);
    }
}
