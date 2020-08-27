using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]

public class InteractibleExternalModel : MonoBehaviour
{
    private void Start()
    {

        // check existence of collider
        if (this.gameObject.GetComponent<Collider>() == null && this.gameObject.GetComponent<XRGrabInteractable>().colliders.Count == 0) Debug.LogError("Missing Collider component on " + this.gameObject.name);

        this.gameObject.layer = 10; // set grabbable layer
        Transform pivot = this.gameObject.transform.Find("Pivot");
        if (pivot != null)
        {
            XRGrabInteractable xRGrabInteractable = this.gameObject.GetComponent<XRGrabInteractable>();
            xRGrabInteractable.attachTransform = pivot.transform;
        }
        else
        {
            Debug.LogWarning("Pivot is missing");
        }
    }
}
