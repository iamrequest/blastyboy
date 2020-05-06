using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// TODO: Add offset for where to respawn grapple point during onRelease()
public class HandGrappler : Grappler {
    private Vector3 originalGrabPointLocalScale;
    public Vector3 originalGrabPointPositionOffset;

    protected override void Start() {
        base.Start();
        doGrabAction.AddOnUpdateListener(DoGrab, parentHand.handType);
        originalGrabPointLocalScale = grabPoint.transform.localScale;
        grabPoint.transform.localPosition += originalGrabPointPositionOffset;
    }

    private void DoGrab(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
        // TODO: This is causing issues when both hands are pressed
        // TODO: Solution - move grab buttons to separate steamvr bool actions
        //if (doGrabAction.trackedDeviceIndex != parentHand.GetDeviceIndex()) return;

        // If we're holding the grab button, and we're hovering a wall
        if (fromAction.stateDown) {
            Collider[] collisions = Physics.OverlapSphere(grabPoint.transform.position,
                grabPoint.transform.lossyScale.x / 2,
                grabbableLayers);

            if (collisions.Length > 0) {
                grabPoint.velocity = Vector3.zero;
                grabPoint.isKinematic = true;
                grabPoint.transform.parent = collisions[0].gameObject.transform;

                OnGrappleStart();
            }
        }

        if (fromAction.state) {
            if (isGrappling) {
                GrappleTo(parentHand.transform.position, grabPoint.transform.position);
            }
        } else {
            if (isGrappling) {
                OnGrappleEnd();
            }
        }
    }

    protected override void OnGrappleStart() {
        base.OnGrappleStart();

        smoothLocomotion.enabled = false;
        smoothLocomotion.activeGrappler = this;
    }

    protected override void OnGrappleEnd() {
        base.OnGrappleEnd();

        smoothLocomotion.enabled = true;

        grabPoint.transform.parent = transform;
        grabPoint.transform.localPosition = originalGrabPointPositionOffset;
        grabPoint.transform.localScale = originalGrabPointLocalScale;
    }
}
