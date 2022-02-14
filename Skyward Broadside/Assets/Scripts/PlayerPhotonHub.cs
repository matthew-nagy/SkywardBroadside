using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhotonHub : MonoBehaviour
{
    // THIS IS PUBLIC FOR NOW, TO ALLOW FINE TUNING DURING TESTING EASIER.
    public float forceToDamageMultiplier = 0.1f;

    private float currHealth;

    //Allow single player testing
    bool disabled;

    //The actual ship of the player
    private GameObject PlayerShip;
    private GuiUpdateScript updateScript;

    // Start is called before the first frame update
    void Start()
    {
        PlayerShip = this.gameObject.transform.GetChild(0).gameObject;
        GameObject userGUI = GameObject.Find("User GUI");
        if(userGUI != null)
        {
            updateScript = userGUI.GetComponent<GuiUpdateScript>();
            disabled = false;
            // We would want a way of accessing the players ship, and fetching the max health of only that. Probably could do it with an enum or something
            currHealth = PlayerShip.GetComponent<ShipArsenal>().maxHealth;
            updateScript.UpdateGUIHealth(currHealth);
        }
        else
        {
            disabled = true;
            Debug.LogWarning("No User GUI could be found (player photon hub constructor)");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHealth(float collisionMagnitude)
    {
        if (!disabled)
        {
            float healthVal = collisionMagnitude * forceToDamageMultiplier;
            currHealth -= healthVal;
            print(currHealth);
            updateScript.UpdateGUIHealth(currHealth);
        }
    }
}
