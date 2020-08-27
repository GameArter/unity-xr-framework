using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace GameArter.XR.ControllerButtonState
{
    // Cross device button input listener - https://docs.unity3d.com/2019.1/Documentation/Manual/xr_input.html

    [System.Serializable]
    public class ButtonsGroup
    {
        public Primary2DAxis primary2DAxis = new Primary2DAxis();
        public Trigger trigger = new Trigger();
        public Grip grip = new Grip();
        public PrimaryButton primaryButton = new PrimaryButton();
        public PrimaryTouch primaryTouch = new PrimaryTouch();
        public SecondaryButton secondaryButton = new SecondaryButton();
        public SecondaryTouch secondaryTouch = new SecondaryTouch();
        public MenuButton menuButton = new MenuButton();
        public Thumbrest thumbrest = new Thumbrest();
        public UserPresence userPresence = new UserPresence();
    }

    // Primary2DAxis && Primary2DAxisClick && Primary2DAxisTouch
    [System.Serializable]
    public class Primary2DAxis
    {
        public Vector2 value = Vector2.zero;
        public bool isTouched = false;
        public bool isPressed = false;
    }

    // Trigger && TriggerButton
    [System.Serializable]
    public class Trigger
    {
        public float value = 0f;
        public bool isPressed = false;
    }

    // Grip && GripButton
    [System.Serializable]
    public class Grip
    {
        public float value = 0f;
        public bool isPressed = false;
    }

    [System.Serializable]
    public class PrimaryButton
    {
        public bool isPressed = false;
    }

    [System.Serializable]
    public class PrimaryTouch
    {
        public bool isPressed = false;
    }

    [System.Serializable]
    public class SecondaryButton
    {
        public bool isPressed = false;
    }

    [System.Serializable]
    public class SecondaryTouch
    {
        public bool isPressed = false;
    }

    [System.Serializable]
    public class MenuButton
    {
        public bool isPressed = false;
    }

    [System.Serializable]
    public class Thumbrest
    {
        public bool isPressed = false;
    }

    [System.Serializable]
    public class UserPresence
    {
        public bool isPresented = false;
    }

    [System.Serializable]
    public class BatteryLevel
    {
        public float value = 1f;
    }
}
