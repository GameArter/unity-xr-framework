using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]

public class XrGrabInteractibleOffset : XRGrabInteractable {

    private Vector3 initialAttachLocalPos;
    private Quaternion initialAttachLocRot;

    void Start(){
        // Create Attach point
        if (!attachTransform)
        {
            GameObject grab = new GameObject("Grab Pivot");
            grab.transform.SetParent(transform, false);
            attachTransform = grab.transform;
        }

        initialAttachLocalPos = attachTransform.localPosition;
        initialAttachLocRot = attachTransform.localRotation;
    }

    protected override void OnSelectEnter(XRBaseInteractor interactor)
    {
        if(interactor is XRDirectInteractor)
        {
            attachTransform.position = interactor.transform.position;
            attachTransform.rotation = interactor.transform.rotation;
        } else
        {
            attachTransform.localPosition = initialAttachLocalPos;
            attachTransform.localRotation = initialAttachLocRot;
        }

        base.OnSelectEnter(interactor);
    }
}
