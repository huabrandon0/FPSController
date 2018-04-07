// Provides a wrapper for inputs; specifically, it maps names to inputs.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
    public static Dictionary<string, KeyCode[]> keyDict = new Dictionary<string, KeyCode[]>()
    {
        {"Strafe Up",       new KeyCode[] {KeyCode.W}},
        {"Strafe Left",     new KeyCode[] {KeyCode.A}},
        {"Strafe Down",     new KeyCode[] {KeyCode.S}},
        {"Strafe Right",    new KeyCode[] {KeyCode.D}},

        {"Jump",            new KeyCode[] {KeyCode.Space}},
        {"Sprint",          new KeyCode[] {KeyCode.LeftShift}},
        {"Crouch",          new KeyCode[] {KeyCode.LeftControl}},

        {"Attack1",         new KeyCode[] {KeyCode.Mouse0}},
        {"Attack2",         new KeyCode[] {KeyCode.Mouse1}}
    };

    public static bool GetKey(string key)
    {
        foreach (KeyCode val in keyDict[key])
        {
            if (Input.GetKey(val))
	            return true;
        }
        return false;
    }

    public static bool GetKeyDown(string key)
    {
        foreach (KeyCode val in keyDict[key])
        {
            if (Input.GetKeyDown(val))
	            return true;
        }
        return false;
    }

    public static bool GetKeyUp(string key)
    {
        foreach (KeyCode val in keyDict[key])
        {
            if (Input.GetKeyUp(val))
	            return true;
        }
        return false;
    }
}
