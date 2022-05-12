using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A single control which can be used as an input
[System.Serializable]
public struct Control
{
    //Keys that activate the control
    public KeyCode primaryKey;
    public KeyCode secondaryKey;

    //Duplicate functions that Unity.Input uses
    public bool IsHeld()
    {
        return Input.GetKey(primaryKey) || Input.GetKey(secondaryKey);
    }

    public bool IsDown()
    {
        return Input.GetKeyDown(primaryKey) || Input.GetKeyDown(secondaryKey);
    }

    //Get a new empty control which won't ever activate
    public static Control Make()
    {
        Control myControl = new Control();
        myControl.primaryKey = KeyCode.None;
        myControl.secondaryKey = KeyCode.None;
        return myControl;
    }

    //Functions to set which keys do what

    public Control SetPrimary(KeyCode primary)
    {
        primaryKey = primary;
        return this;
    }
    public Control SetSecondary(KeyCode secondary)
    {
        secondaryKey = secondary;
        return this;
    }

}

//Where all controls are stored
public static class SBControls
{
    //Codes to refer to rebindable controls
    [System.Serializable]
    public enum ControlCode
    {
        Forwards, Backwards, Left, Right, Shoot, LockOn, Ammo1, Ammo2, Ammo3, YAxisUp, YAxisDown, ShootOption2
    };

    //Used in control binding. Given a control code and they key to change it to, it will change its primary key
    public static void SetControlTo(ControlCode toChange, KeyCode changeTo)
    {
        switch (toChange)
        {
            case ControlCode.Forwards:
                forwards.primaryKey = changeTo;
                break;
            case ControlCode.Backwards:
                backwards.primaryKey = changeTo;
                break;
            case ControlCode.Left:
                left.primaryKey = changeTo;
                break;
            case ControlCode.Right:
                right.primaryKey = changeTo;
                break;
            case ControlCode.Ammo1:
                ammo1.primaryKey = changeTo;
                break;
            case ControlCode.Ammo2:
                ammo2.primaryKey = changeTo;
                break;
            case ControlCode.Ammo3:
                ammo3.primaryKey = changeTo;
                break;
            case ControlCode.YAxisUp:
                yAxisUp.primaryKey = changeTo;
                break;
            case ControlCode.YAxisDown:
                yAxisDown.primaryKey = changeTo;
                break;
            case ControlCode.ShootOption2:
                shoot.secondaryKey = changeTo;
                break;
        }
    }

    //Movement controls
    public static Control forwards = Control.Make().SetPrimary(KeyCode.W);
    public static Control backwards = Control.Make().SetPrimary(KeyCode.S);
    public static Control left = Control.Make().SetPrimary(KeyCode.A);
    public static Control right = Control.Make().SetPrimary(KeyCode.D);

    //Weaponary controls
    public static Control shoot = Control.Make().SetPrimary(KeyCode.Mouse0).SetSecondary(KeyCode.Space);
    public static Control lockOn = Control.Make().SetPrimary(KeyCode.Mouse1);

    //Ammo selection
    public static Control ammo1 = Control.Make().SetPrimary(KeyCode.Alpha1);
    public static Control ammo2 = Control.Make().SetPrimary(KeyCode.Alpha2);
    public static Control ammo3 = Control.Make().SetPrimary(KeyCode.Alpha3);

    //Get the scoreboard up
    public static Control viewScoreboard = Control.Make().SetPrimary(KeyCode.Tab).SetSecondary(KeyCode.Tab);
  
    //Controls to go up and down
    public static Control yAxisUp = Control.Make().SetPrimary(KeyCode.R).SetSecondary(KeyCode.Q);
    public static Control yAxisDown = Control.Make().SetPrimary(KeyCode.F).SetSecondary(KeyCode.E);
}
