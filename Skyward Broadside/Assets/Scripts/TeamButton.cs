using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamButton : MonoBehaviour
{
    public static string joinTeam;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Image>().color = Color.red;
        joinTeam = "Red";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Clicked()
    {
        if (joinTeam == "Red")
        {
            joinTeam = "Blue";
            GetComponent<Image>().color = Color.blue;
        }
        else if (joinTeam == "Blue")
        {
            joinTeam = "Red";
            GetComponent<Image>().color = Color.red;
        }
    }
}
