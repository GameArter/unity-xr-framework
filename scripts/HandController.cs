using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using GameArter.XR.ControllerButtonState;

public class HandController : MonoBehaviour
{
    public ControllerNode controllerNode;
    private InputDevice targetDevice;
    public ControllerConf controller;
    public HandConf hand;
    private byte initAttemptsPool = 50;
    private bool controllerDisplayRequest;

    void Start()
    {
        controllerDisplayRequest = controller.display;
        controller.display = true;
        StartCoroutine(Init(0));
    }

    void Update()
    {
        if (!controller.display) UpdateHandAnimation();

        // switch controller / hand on pressed secondary button
        targetDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButton);
        if (secondaryButton != controller.inputs.secondaryButton.isPressed)
        {
            if (secondaryButton)
            {
                controller.display = !controller.display;
                SetVisual((controller.display) ? Visual.controller : Visual.hand);
            }
            controller.inputs.secondaryButton.isPressed = secondaryButton;
        }
    }

    void UpdateHandAnimation()
    {
        hand.animator.SetFloat("Trigger", (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float tv)) ? tv : 0);
        hand.animator.SetFloat("Grip", (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gv)) ? gv : 0);
    }

    public void SetVisual(Visual visual)
    {
        controller.target.SetActive(visual != Visual.hand);
        hand.target.SetActive(visual == Visual.hand);
    }

    private IEnumerator Init(byte waitFor)
    {
        initAttemptsPool--;
        if(initAttemptsPool > 0)
        {
            yield return new WaitForSeconds(waitFor);

            if (controller.display && controller.models.Length == 0) throw new System.Exception("No controller model to display. Fill controller models into HandController");

            List<InputDevice> devices = new List<InputDevice>();
            InputDeviceCharacteristics controllerCharacteristics = InputDeviceCharacteristics.Controller | ((controllerNode == ControllerNode.LeftHand) ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right);
            InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

            GameObject controllerPrefab = controller.models[0];
            if (devices.Count > 0)
            {
                targetDevice = devices[0];
                GameObject realController = System.Array.Find(controller.models, controller => controller.name == targetDevice.name);
                if (realController)
                {
                    controllerPrefab = realController;
                }
                else
                {
                    Debug.LogWarning("Corresponding controller was not found!");
                }
            }
            else
            {
                Debug.LogError("No input device controller found!");
            }

            if (!targetDevice.isValid) { 
                StartCoroutine(Init(1)); // try again after a second
            } else
            {
                hand.target = Instantiate(hand.model, transform);
                hand.animator = hand.target.GetComponent<Animator>();

                controller.target = Instantiate(controllerPrefab, transform);
                controller.display = controllerDisplayRequest;

                SetVisual((controller.display) ? Visual.controller : Visual.hand);
            }
        } else
        {
            throw new System.Exception("Controller initialization attempts failed");
        }
        yield break;
    }

    public enum ControllerNode
    {
        RightHand,
        LeftHand
    }
    public enum Visual
    {
        hand,
        controller
    }
    [System.Serializable]
    public class ControllerConf
    {
        public bool display;
        public GameObject[] models;
        internal GameObject target;
        public ButtonsGroup inputs = new ButtonsGroup();
    }
    [System.Serializable]
    public class HandConf
    {
        public GameObject model;
        internal Animator animator;
        internal GameObject target;
    }
}
