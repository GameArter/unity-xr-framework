using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using GameArter.XR.ControllerButtonState;
using UnityEngine.UI;

namespace GameArter.XR
{
    [RequireComponent(typeof(XRRig))]
    [RequireComponent(typeof(CharacterController))]

    public class XRRigManager : MonoBehaviour
    {
        public bool debugMode = false;

        private XRRig xrRig;
        private CharacterController character;

        [SerializeField]
        private XRUser xrUser = new XRUser();
        [SerializeField]
        private HandControllerSetup rightController = new HandControllerSetup();
        [SerializeField]
        private HandControllerSetup leftController = new HandControllerSetup();

        private MotionRam motionRam = new MotionRam();
        private ClimbingRam climbingRam = new ClimbingRam();

        public static XRRigManager I;

        void Awake()
        {
            if (this.enabled)
            {
                if (this.gameObject.layer != 9)
                {
                    this.gameObject.layer = 9;
                    Debug.LogWarning("Layer of XR Rig has been set to index 9 by XR Framework");
                }
                xrRig = this.gameObject.GetComponent<XRRig>();
                character = this.gameObject.GetComponent<CharacterController>();

                rightController.xrNode = XRNode.RightHand;
                leftController.xrNode = XRNode.LeftHand;
                // teleportation enabled
                if (rightController.teleportation.enabled) TeleportationInit(rightController);
                if (leftController.teleportation.enabled) TeleportationInit(leftController);


                // continuous movement enabled
                if (rightController.continuousMovement.enabled || leftController.continuousMovement.enabled) ContinuousMovementInit();
                // jump movement enabled
                if (rightController.jumpMovement.enabled || leftController.jumpMovement.enabled) JumpMovementInit();

                // Snap turning enabled
                if (rightController.snapTurning.enabled || leftController.snapTurning.enabled) SnapTurningInit();


                // Set active movement
                SetActiveMovementSystem(rightController);
                SetActiveMovementSystem(leftController);

                // Climbing enabled
                if (rightController.climbingEnabled || leftController.climbingEnabled) climbingRam.enabled = true;

                if (!xrUser.hatDropZone)
                {
                    Transform hatZone = xrRig.cameraGameObject.transform.Find("HatDropZone");
                    if (hatZone != null) hatZone.gameObject.SetActive(false);
                }

                I = this;
            }
        }

        private void Move(Vector3 motion)
        {
            character.Move(motion);
            CapsuleFollowHeadset();
        }

        private void CapsuleFollowHeadset()
        {
            character.height = xrRig.cameraInRigSpaceHeight; // + additionalHeight;
            Vector3 capsuleCenter = transform.InverseTransformPoint(xrRig.cameraGameObject.transform.position);
            character.center = new Vector3(capsuleCenter.x, character.height / 2, capsuleCenter.z);
        }



        void SetActiveMovementSystem(HandControllerSetup controller)
        {
            if (controller.enabledMovements.Count > 0)
            {
                controller.activeMovement = controller.enabledMovements[0];
                if(debugMode) Debug.Log("Controller | Active Locomotion options: " + controller.enabledMovements.Count);
            }
            else
            {
                Debug.LogWarning("No motion system attached to controller");
            }
        }

        void ContinuousMovementInit()
        {
            motionRam.continuousMovement = false;
            if (rightController.continuousMovement.enabled)
            {
                rightController.enabledMovements.Add(HandControllerSetup.ActiveMovement.continuous);
                if (rightController.continuousMovement.activeThreshold < 0.2f) Debug.LogWarning("Active threshold for continuous movement is too low on right controller");
            }

            if (leftController.continuousMovement.enabled)
            {
                leftController.enabledMovements.Add(HandControllerSetup.ActiveMovement.continuous);
                if (leftController.continuousMovement.activeThreshold < 0.2f) Debug.LogWarning("Active threshold for continuous movement is too low on left controller");
            }
        }

        void JumpMovementInit()
        {
            if (rightController.jumpMovement.enabled)
            {
                rightController.enabledMovements.Add(HandControllerSetup.ActiveMovement.jump);
                if (rightController.jumpMovement.activeThreshold < 0.2f) Debug.LogWarning("Active threshold for jump movement is too low on right controller");
            }

            if (leftController.jumpMovement.enabled)
            {
                leftController.enabledMovements.Add(HandControllerSetup.ActiveMovement.jump);
                if (leftController.jumpMovement.activeThreshold < 0.2f) Debug.LogWarning("Active threshold for jump movement is too low on left controller");
            }
        }

        void SnapTurningInit()
        {
            motionRam.snapProvider = this.gameObject.GetComponent<SnapTurnProvider>();
            if (motionRam.snapProvider == null)
            {
                Debug.LogWarning("Missing SnapTurnProvider for XR Rig");
            } else
            {
                // is not active by default
                motionRam.snapProvider.enabled = false;
            }
            if (rightController.snapTurning.enabled) rightController.enabledMovements.Add(HandControllerSetup.ActiveMovement.snap);
            if (leftController.snapTurning.enabled) leftController.enabledMovements.Add(HandControllerSetup.ActiveMovement.snap);
        }

        private void Update()
        {
            ManageMovement(rightController);
            ManageMovement(leftController);

            MotionOptionSwitcher(rightController);
            MotionOptionSwitcher(leftController);
        }

        void MotionOptionSwitcher(HandControllerSetup c)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(c.xrNode);

            device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool prevPrimary2DAxisPressed);
            if (prevPrimary2DAxisPressed != c.inputs.primary2DAxis.isPressed)
            {
                if (prevPrimary2DAxisPressed)
                {
                    if (c.activeMovement == HandControllerSetup.ActiveMovement.continuous) motionRam.continuousMovement = false;
                    
                    int msi = c.enabledMovements.IndexOf(c.activeMovement);
                    if(debugMode) Debug.Log("Switched motion system for " + c.xrNode.ToString() + "C index: " + msi + " / " + c.enabledMovements.Count);
                    msi = (msi < c.enabledMovements.Count - 1) ? (msi + 1) : 0;
                    c.activeMovement = c.enabledMovements[msi];

                    // Activate / deactivate objects
                    if (c.snapTurning.enabled) this.gameObject.GetComponent<SnapTurnProvider>().enabled = (c.activeMovement == HandControllerSetup.ActiveMovement.snap);

                    if (c.activeMotionUI != null)
                    {
                        c.activeMotionUI.text = "Locomotion:" + c.activeMovement.ToString();
                        StartCoroutine(MotionUi(c.activeMotionUI.transform.parent.gameObject));
                    }
                }
                c.inputs.primary2DAxis.isPressed = prevPrimary2DAxisPressed;
            }
        }

        private IEnumerator MotionUi(GameObject ui)
        {
            ui.SetActive(true);
            yield return new WaitForSeconds(1f);
            ui.SetActive(false);
            yield break;
        }

        void ManageMovement(HandControllerSetup c)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(c.xrNode);

            // continuous / jump
            switch (c.activeMovement)
            {
                case HandControllerSetup.ActiveMovement.teleportation:
                    if (c.teleportation.enabled)
                    {
                        Vector2 primary2DAxisValue;
                        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxisValue); // update input axis
                        bool isActivated = (Math.Abs(primary2DAxisValue.x) >= c.teleportation.activeThreshold || Math.Abs(primary2DAxisValue.y) >= c.teleportation.activeThreshold);
                        if (isActivated)
                        {
                            Quaternion headYaw = Quaternion.Euler(0, xrRig.cameraGameObject.transform.eulerAngles.y, 0);
                            Transform teleportCircle = c.teleportation.rayInteractor.transform.Find("teleport-circle");
                            if(teleportCircle != null)
                            {
                                float angle = 0f;

                                if (primary2DAxisValue.x < 0)
                                {
                                    angle = 360 - (Mathf.Atan2(primary2DAxisValue.x, primary2DAxisValue.y) * Mathf.Rad2Deg * -1);
                                }
                                else
                                {
                                    angle = Mathf.Atan2(primary2DAxisValue.x, primary2DAxisValue.y) * Mathf.Rad2Deg;
                                }

                                if(debugMode) Debug.Log("angle " + angle);
                                teleportCircle.gameObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
                            }
                        }
                        c.teleportation.isActive = isActivated;
                        c.teleportation.rayInteractor.SetActive(isActivated);
                    }
                    break;
                case HandControllerSetup.ActiveMovement.continuous:
                    device.TryGetFeatureValue(CommonUsages.primary2DAxis, out motionRam.continuousInputAxis); // update input axis
                    if (Math.Abs(motionRam.continuousInputAxis.x) >= c.continuousMovement.activeThreshold || Math.Abs(motionRam.continuousInputAxis.y) >= c.continuousMovement.activeThreshold)
                    {
                        motionRam.activeContinuousMovement = c.continuousMovement;
                        motionRam.continuousMovement = true;
                    }
                    else
                    {
                        motionRam.continuousMovement = false;
                    }
                    break;
                case HandControllerSetup.ActiveMovement.jump:
                    device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 jumpInputDirection); // update input axis
                    bool isTouched = (Math.Abs(jumpInputDirection.x) >= c.jumpMovement.activeThreshold || Math.Abs(jumpInputDirection.y) >= c.jumpMovement.activeThreshold);
                    if(isTouched != c.inputs.primary2DAxis.isTouched)
                    {
                        c.inputs.primary2DAxis.isTouched = isTouched;
                        if (isTouched)
                        {
                            Quaternion headYaw = Quaternion.Euler(0, xrRig.cameraGameObject.transform.eulerAngles.y, 0);
                            Vector3 direction = headYaw * new Vector3(jumpInputDirection.x + c.jumpMovement.distance, 0, jumpInputDirection.y + c.jumpMovement.distance);
                            Move(direction);
                        }
                    }
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (!xrUser.gravitation.active || (!rightController.teleportation.isActive && !leftController.teleportation.isActive))
            {
                if (climbingRam.climbingHand)
                {
                    // climbing computation
                    InputDevices.GetDeviceAtXRNode(climbingRam.climbingHand.controllerNode).TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity);
                    Move(transform.rotation * -velocity * Time.fixedDeltaTime);
                }
                else
                {
                    if (motionRam.continuousMovement)
                    {
                        if (motionRam.activeContinuousMovement.speed <= 0.1f) Debug.LogWarning("Motion speed " + motionRam.activeContinuousMovement.speed + " is low");
                        Quaternion headYaw = Quaternion.Euler(0, xrRig.cameraGameObject.transform.eulerAngles.y, 0); // rotation
                        Vector3 direction = headYaw * new Vector3(motionRam.continuousInputAxis.x, 0, motionRam.continuousInputAxis.y) * Time.deltaTime * motionRam.activeContinuousMovement.speed;
                        Move(direction);
                    }

                    if (xrUser.gravitation.active) ApplyGravity();
                }
            } else {
                Debug.Log("Motion Ram is teleportation");
            }            
        }

        private void ApplyGravity()
        {
            // check if grounded
            Vector3 rayStart = transform.TransformPoint(character.center);
            float rayLength = character.center.y + 0.025f;
            if (!Physics.SphereCast(rayStart, character.radius, Vector3.down, out RaycastHit hitInfo, rayLength, xrUser.gravitation.groundLayer))
            {
                if(debugMode) Debug.Log("XRRig is falling");
                xrUser.gravitation.fallingSpeed += xrUser.gravitation.acceleration * Time.fixedDeltaTime;
                Move(Vector3.up * xrUser.gravitation.fallingSpeed * Time.fixedDeltaTime);
            } else if(xrUser.gravitation.fallingSpeed != 0)
            {
                xrUser.gravitation.fallingSpeed = 0;
            }
        }

        public void OnSelectEnter(string action, XRBaseInteractor interactor)
        {
            if(action == "climbing")
            {
                climbingRam.climbingHand = interactor.GetComponent<XRController>();
            }
        }

        public void OnSelectExit(string action, XRBaseInteractor interactor)
        {
            if (action == "climbing")
            {
                if (climbingRam.climbingHand && climbingRam.climbingHand.name == interactor.name) climbingRam.climbingHand = null;
            }
        }

        [System.Serializable]
        private class XRUser
        {
            public Gravity gravitation = new Gravity();
            public bool hatDropZone = false;

            [System.Serializable]
            public class Gravity
            {
                public bool active = false;
                public LayerMask groundLayer = 8;
                public float acceleration = -9.81f;
                internal float fallingSpeed = 0;
            }
        }

        [System.Serializable]
        private class HandControllerSetup
        {
            internal XRNode xrNode;
            public GameObject directInteractor = null;
            public Text activeMotionUI = null;
            public Teleportation teleportation = new Teleportation();
            public XRRayInteractor rayInteractor = null;
            internal HoveringRam hoveringRam = new HoveringRam();
            public ContinuousMovement continuousMovement = new ContinuousMovement();
            public JumpMovement jumpMovement = new JumpMovement();
            public SharedOptions snapTurning = new SharedOptions();
            public bool climbingEnabled = true;
            internal ActiveMovement activeMovement = new ActiveMovement();
            internal ButtonsGroup inputs = new ButtonsGroup();
            internal List<ActiveMovement> enabledMovements = new List<ActiveMovement>();

            [System.Serializable]
            internal enum ActiveMovement
            {
                teleportation,
                continuous,
                jump,
                snap
            }

            [System.Serializable]
            public class SharedOptions
            {
                public bool enabled = false; // enable feature in the project
            }
            [System.Serializable]
            public class ContinuousMovement : SharedOptions
            {
                [Tooltip("Set speed > 0 for continuous movement")]
                public float speed = 1f;
                [Tooltip("Touch value for activation")]
                public float activeThreshold = 0.2f;
            }

            [System.Serializable]
            public class JumpMovement : SharedOptions
            {
                [Tooltip("Set disatnce of teh jump movement")]
                public float distance = 50f;
                [Tooltip("Touch value for activation")]
                public float activeThreshold = 0.2f;
            }

            [System.Serializable]
            public class Teleportation : SharedOptions
            {
                public GameObject rayInteractor = null;
                internal XRController xrController = null;
                internal float activeThreshold = 0.2f; // visibility manager
                internal bool isActive;
            }
        }

        [System.Serializable]
        private class MotionRam
        {
            public bool continuousMovement = false;
            public HandControllerSetup.ContinuousMovement activeContinuousMovement = new HandControllerSetup.ContinuousMovement();
            public Vector2 continuousInputAxis = Vector2.zero;
            public SnapTurnProvider snapProvider;
        }

        [System.Serializable]
        private class HoveringRam
        {
            public Vector3 pos = new Vector3();
            public Vector3 norm = new Vector3();
            public int index = 0;
            public bool validTarget = false;
        }

        [System.Serializable]
        private class ClimbingRam
        {
            internal bool enabled = false;
            internal XRController climbingHand;
        }

        private void TeleportationInit(HandControllerSetup c)
        {
            // Add required sxripts to XRrig, if missing
            if (this.gameObject.GetComponent<LocomotionSystem>() == null) Debug.LogError("Locomotion system placed by XrControllerManager");
            if (this.gameObject.GetComponent<TeleportationProvider>() == null) Debug.LogError("Teleportation provider placed by XrControllerManager");

            // get activation button and threshold
            if (c.teleportation.rayInteractor != null)
            {
                c.teleportation.xrController = c.teleportation.rayInteractor.GetComponent<XRController>();

                if (c.teleportation.xrController.selectUsage != InputHelpers.Button.Primary2DAxisTouch) Debug.LogError("Select usage of Teleport ray must be set to Primary 2D Axis Touch");
                c.teleportation.activeThreshold = c.teleportation.xrController.axisToPressThreshold;

                c.enabledMovements.Add(HandControllerSetup.ActiveMovement.teleportation);
                c.teleportation.rayInteractor.SetActive(false);
            }
            else
            {
                Debug.LogError("Missing teleport ray object in Xr Hand Controller Manager");
            }
        }
    }
}