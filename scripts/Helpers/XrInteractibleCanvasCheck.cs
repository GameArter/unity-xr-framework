using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Place this script into canvas element. It will check whether all is set properly

public class XrInteractibleCanvasCheck : MonoBehaviour
{

    public GameObject eventSystem;
    public GameObject mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        if (this.gameObject.GetComponent<TrackedDeviceGraphicRaycaster>() == null) Debug.LogError("Missing TrackedDeviceGraphicRaycaster component in Canvas element");
        if (eventSystem != null)
        {
            if (eventSystem.GetComponent<XRUIInputModule>() == null) Debug.LogError("Missing XRUIInputModule component in EventSystem element");
        }
        else
        {
            Debug.LogError("Missing event system in the scene");
        }

        if (mainCamera.tag != "MainCamera") Debug.LogWarning("Main Camera object has not attached tag Main Camera");
    }
}
