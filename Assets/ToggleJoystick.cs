using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleJoystick : MonoBehaviour {

    public SimpleInputNamespace.Joystick Joystick;
    public Image joystickBackground;

    public void ToggleJoystickFloatingMode ()
    {
        Joystick.ToggleDynamicJoystick();
        joystickBackground.enabled = Joystick.isDynamicJoystick;
    }
}
