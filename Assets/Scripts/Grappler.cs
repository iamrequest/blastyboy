using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Grappler : MonoBehaviour {
    private Hand parentHand;
    private Blaster blaster;
    private LineRenderer lineRenderer;
    public Vector3 motion; // The current momentum applied by the grappler

    [Header("Config")]
    public CharacterController characterController;
    public SteamVR_Action_Boolean drawGrabAction;
    public SteamVR_Action_Boolean doGrabAction;
    public GameObject grabPoint;
    public SmoothLocomotion smoothLocomotion;

    [Header("Grappling")]
    public float maxGrabDistance;
    public LayerMask grabbableLayers;
    public float pullSpeed;
    public float stopDistance;

    public bool conserveMomentumOnGrappleStop;

    // Start is called before the first frame update
    void Start() {
        blaster = GetComponent<Blaster>();
        lineRenderer = GetComponent<LineRenderer>();
        parentHand = GetComponentInParent<Hand>();

        drawGrabAction.AddOnUpdateListener(DrawGrab, parentHand.handType);
        doGrabAction.AddOnUpdateListener(DoGrab, parentHand.handType);
    }

    private void Update() {
        Debug.DrawRay(blaster.spawnTransform.position, motion);
        if (characterController.isGrounded) {
            motion = Vector3.zero;
        }
    }

    // TODO: Bug fixes:
    //  - Player can pull themselves to a ceiling/wall, and pull themselves through
    //  - At the point where the player is at the grapple point, it's choppy
    //      - This is because the stopDistance is a firm cutoff
    public void GrappleTo(Vector3 fromPosition, Vector3 targetPosition) {
        Vector3 dirToTarget = targetPosition - fromPosition;

        if (dirToTarget.magnitude > stopDistance) {
            motion = dirToTarget.normalized * pullSpeed;
            characterController.Move(motion * Time.deltaTime);
        } else {
            motion = Vector3.zero;
        }
    }

    private void DrawGrab(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
        if (newState) {
            // If we're in the middle of grappling to something, don't draw the line renderer, and stop here.
            if (doGrabAction.trackedDeviceIndex == parentHand.GetDeviceIndex() 
                && doGrabAction.state 
                && grabPoint.activeInHierarchy) {
                lineRenderer.enabled = false;

                return;
            }

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, blaster.spawnTransform.position);
            lineRenderer.SetPosition(1, blaster.spawnTransform.position + transform.forward * maxGrabDistance);

            // Raycast the grab point
            RaycastHit rayHit;
            if (Physics.Raycast(blaster.spawnTransform.position, transform.forward, out rayHit, maxGrabDistance, grabbableLayers)) {
                grabPoint.SetActive(true);

                grabPoint.transform.position = rayHit.point;
            } else {
                grabPoint.SetActive(false);
            }
        } else {
            lineRenderer.enabled = false;
            grabPoint.SetActive(false);
        }
    }

    // TODO: Only grapple to certain gameobjects
    // TODO: Add force grab
    private void DoGrab(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
        // If we're in the middle of pulling ourselves towards the grapple point, disable smooth locomotion.
        if (newState && grabPoint.activeInHierarchy) {
            smoothLocomotion.enabled = false;
            GrappleTo(blaster.spawnTransform.position, grabPoint.transform.position);
        } else {
            smoothLocomotion.enabled = true;
        }
    }

    public bool IsCurrentlyGrappling() {
        return (drawGrabAction.trackedDeviceIndex == parentHand.GetDeviceIndex() && drawGrabAction.state
            || doGrabAction.trackedDeviceIndex == parentHand.GetDeviceIndex() && doGrabAction.state);
    }

    // If we got halfway through a grab, we should maintain the momentum from the pull
    //  until we hit the ground
    public Vector3 getLeftoverMomentum() {
        if (conserveMomentumOnGrappleStop) {
            return motion;
        } else {
            return Vector3.zero;
        }
    }

    // Apply a downward force to our leftover momentum from a force grab
    public void ApplyGravity(float gravity) {
        motion.y -= gravity * Time.deltaTime;
    }
}
