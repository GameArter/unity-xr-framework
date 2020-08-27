using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GameArter.XR;

[RequireComponent(typeof(Rigidbody))]

// Place this script on climb stones / objects
public class ClimbInteractible : XRBaseInteractable
{

    void Start()
    {
        if (this.gameObject.GetComponent<Collider>() == null) Debug.LogError("Missing Collider component on " + this.gameObject.name);

        this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        this.gameObject.layer = 11;
    }

    protected override void OnSelectEnter(XRBaseInteractor interactor)
    {
        base.OnSelectEnter(interactor);
        if(interactor is XRDirectInteractor) XRRigManager.I.OnSelectEnter("climbing", interactor); 
    }

    protected override void OnSelectExit(XRBaseInteractor interactor)
    {
        base.OnSelectExit(interactor);
        XRRigManager.I.OnSelectExit("climbing", interactor);
    }
}
