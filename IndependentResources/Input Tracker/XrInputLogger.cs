using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR;
using GameArter.XR.ControllerButtonState;


namespace GameArter.XR.InputPrinter
{
    public class XrInputLogger : MonoBehaviour
    {
        public GameObject logObject;
        public int fontSize = 40;
        public int lineHeight = 70;
        public int headerOffset = 40;

        [System.Serializable]
        public class EventListener
        {
            [SerializeField]
            public XRNode[] xrNodes = { XRNode.LeftHand, XRNode.RightHand };
            internal ButtonsGroup[] buttonStates;
            public List<InputDevice> devices = new List<InputDevice>();
            internal Device[] device;
            internal int nodes;

            [System.Serializable]
            internal class Device
            {
                public string name;
                public InputDevice input;
            }
        }
        public EventListener listener;

        [System.Serializable]
        private class PrintManager
        {
            public List<String> inputIds = new List<string>() { };
            public List<Text> textInputs = new List<Text>() { };
        }
        private PrintManager printManager = new PrintManager() { };

        void Start()
        {

            // set proper pivot
            RectTransform tort = this.gameObject.GetComponent<RectTransform>();
            if (tort != null) tort.pivot = new Vector2(.5f, 1);
            listener.nodes = listener.xrNodes.Length;
            listener.buttonStates = new ButtonsGroup[listener.nodes];
            listener.device = new EventListener.Device[listener.nodes];
            for (int i = 0; i < listener.nodes; i++)
            {
                listener.buttonStates[i] = new ButtonsGroup();
            }
            GetDevice();
        }

        void GetDevice()
        {
            for (int i = 0; i < listener.nodes; i++)
            {
                InputDevices.GetDevicesAtXRNode(listener.xrNodes[i], listener.devices);
                listener.device[i] = new EventListener.Device
                {
                    name = listener.xrNodes[i].ToString(),
                    input = listener.devices.FirstOrDefault()
                };
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < listener.nodes; i++)
            {
                if (!listener.device[i].input.isValid) GetDevice();
            }
        }

        void Update()
        {
            if (listener.device == null)
            {
                Debug.LogWarning("device input is null");
                Print("", "device not found", true, XRNode.Head);
                return;
            }
            for (int i = 0; i < listener.nodes; i++)
            {
                if (!listener.device[i].input.isValid)
                {
                    GetDevice();
                }
                else
                {
                    inputListener(listener.device[i].input, listener.xrNodes[i], listener.buttonStates[i]);
                }
            }
        }



        private void inputListener(InputDevice device, XRNode xRNode, ButtonsGroup buttonStates)
        {
            if (!device.isValid) GetDevice();

            // XR Input Mapping - https://docs.unity3d.com/Manual/xr_input.html

            // CommonUsages.primary2DAxis
            device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxisValue);
            if (buttonStates.primary2DAxis.value != primary2DAxisValue)
            {
                buttonStates.primary2DAxis.value = primary2DAxisValue;
                Print("primary2DAxis", buttonStates.primary2DAxis.value.ToString(), (buttonStates.primary2DAxis.value.x > 0.01f || buttonStates.primary2DAxis.value.y > 0.01f), xRNode);
            }

            // CommonUsages.primary2DAxisClick
            bool prevPrimary2DAxisClickValue = buttonStates.primary2DAxis.isPressed;
            device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out buttonStates.primary2DAxis.isPressed);
            if (buttonStates.primary2DAxis.isPressed != prevPrimary2DAxisClickValue) Print("primary2DAxisClick", buttonStates.primary2DAxis.isPressed.ToString(), buttonStates.primary2DAxis.isPressed, xRNode);

            // CommonUsages.primary2DAxisTouch
            bool prevPrimary2DAxisTouchValue = buttonStates.primary2DAxis.isTouched;
            device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out buttonStates.primary2DAxis.isTouched);
            if (buttonStates.primary2DAxis.isPressed != prevPrimary2DAxisTouchValue) Print("primary2DAxisTouch", buttonStates.primary2DAxis.isTouched.ToString(), buttonStates.primary2DAxis.isTouched, xRNode);

            // CommonUsages.trigger
            float prevTriggerValue = buttonStates.trigger.value;
            device.TryGetFeatureValue(CommonUsages.trigger, out buttonStates.trigger.value);
            if (buttonStates.trigger.value != prevTriggerValue) Print("Trigger", buttonStates.trigger.value.ToString(), (buttonStates.trigger.value != 0f), xRNode);

            // CommonUsages.triggerButton
            bool prevTriggerButtonValue = buttonStates.trigger.isPressed;
            device.TryGetFeatureValue(CommonUsages.triggerButton, out buttonStates.trigger.isPressed);
            if (buttonStates.trigger.isPressed != prevTriggerButtonValue) Print("TriggerButton", buttonStates.trigger.isPressed.ToString(), buttonStates.trigger.isPressed, xRNode);

            // CommonUsages.grip
            float prevGripValue = buttonStates.grip.value;
            device.TryGetFeatureValue(CommonUsages.grip, out buttonStates.grip.value);
            if (buttonStates.grip.value != prevGripValue) Print("Grip", buttonStates.grip.value.ToString(), buttonStates.grip.value != 0f, xRNode);

            // CommonUsages.gripButton
            bool prevGripButtonValue = buttonStates.grip.isPressed;
            device.TryGetFeatureValue(CommonUsages.gripButton, out buttonStates.grip.isPressed);
            if (buttonStates.grip.isPressed != prevGripButtonValue) Print("GripButton", buttonStates.grip.isPressed.ToString(), buttonStates.grip.isPressed, xRNode);

            // CommonUsages.primaryButton
            bool prevPrimaryButtonValue = buttonStates.primaryButton.isPressed;
            device.TryGetFeatureValue(CommonUsages.primaryButton, out buttonStates.primaryButton.isPressed);
            if (buttonStates.primaryButton.isPressed != prevPrimaryButtonValue) Print("PrimaryButton", buttonStates.primaryButton.isPressed.ToString(), buttonStates.primaryButton.isPressed, xRNode);

            // CommonUsages.primaryTouch
            bool prevPrimaryTouchValue = buttonStates.primaryTouch.isPressed;
            device.TryGetFeatureValue(CommonUsages.primaryTouch, out buttonStates.primaryTouch.isPressed);
            if (buttonStates.primaryTouch.isPressed != prevPrimaryTouchValue) Print("PrimaryTouch", buttonStates.primaryTouch.isPressed.ToString(), buttonStates.primaryTouch.isPressed, xRNode);

            // CommonUsages.secondarzButton
            bool prevSecondaryButtonValue = buttonStates.secondaryButton.isPressed;
            device.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonStates.secondaryButton.isPressed);
            if (buttonStates.secondaryButton.isPressed != prevSecondaryButtonValue) Print("SecondaryButton", buttonStates.secondaryButton.isPressed.ToString(), buttonStates.secondaryButton.isPressed, xRNode);

            // CommonUsages.secondaryTouch
            bool prevSecondaryTouchValue = buttonStates.secondaryTouch.isPressed;
            device.TryGetFeatureValue(CommonUsages.secondaryTouch, out buttonStates.secondaryTouch.isPressed);
            if (buttonStates.secondaryTouch.isPressed != prevSecondaryTouchValue) Print("SecondaryTouch", buttonStates.secondaryTouch.isPressed.ToString(), buttonStates.secondaryTouch.isPressed, xRNode);

            // CommonUsages.menuButton
            bool prevMenuButtonhValue = buttonStates.menuButton.isPressed;
            device.TryGetFeatureValue(CommonUsages.menuButton, out buttonStates.menuButton.isPressed);
            if (buttonStates.menuButton.isPressed != prevMenuButtonhValue) Print("MenuButton", buttonStates.menuButton.isPressed.ToString(), buttonStates.menuButton.isPressed, xRNode);

            // CommonUsages.thumbrest
            bool prevUserPresenceValue = buttonStates.userPresence.isPresented;
            device.TryGetFeatureValue(CommonUsages.userPresence, out buttonStates.userPresence.isPresented);
            if (buttonStates.userPresence.isPresented != prevUserPresenceValue) Print("UserPresence", buttonStates.userPresence.isPresented.ToString(), buttonStates.userPresence.isPresented, xRNode);
        }

        private void Print(string buttonId, string value, bool isActive, XRNode xRNode)
        {
            string key = buttonId + "-" + xRNode.ToString();
            // Find input space
            int index = printManager.inputIds.IndexOf(key);
            Text inputSpace;
            if (index > -1)
            {
                inputSpace = printManager.textInputs[index];
            }
            else
            {
                inputSpace = CreateTextInput(printManager.inputIds.Count);
                printManager.inputIds.Add(key);
                printManager.textInputs.Add(inputSpace);
            }
            inputSpace.color = (isActive) ? Color.green : Color.red;
            inputSpace.text = ($"[{buttonId} | {xRNode}] {value}");
        }

        private Text CreateTextInput(int lineIndex)// controller
        {
            GameObject newLine = new GameObject("line " + lineIndex);
            newLine.transform.SetParent(logObject.transform);

            Text txt = newLine.AddComponent<Text>();
            txt.color = Color.white;
            txt.fontSize = fontSize;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.alignment = TextAnchor.MiddleCenter;

            RectTransform rt = txt.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(967, lineHeight);
            rt.localScale = new Vector2(1, 1);
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.localPosition = new Vector3(0, lineIndex * lineHeight * -1 - headerOffset, 0);
            return txt;
        }
    }
}

