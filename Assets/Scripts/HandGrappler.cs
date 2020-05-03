using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// TODO: Hand grapple isn't working
public class HandGrappler : Grappler {
    //protected GameObject grabPoint;

    protected override void Start() {
        base.Start();
        doGrabAction.AddOnUpdateListener(DoGrab, parentHand.handType);
    }

    private void DoGrab(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
        if (doGrabAction.trackedDeviceIndex != parentHand.GetDeviceIndex()) return;

        // If the player lets go of the grab button
        if (!newState) {
            smoothLocomotion.enabled = true;
            return;
        }

        // Test if the grab point is hovering anything
        RaycastHit rayHit;


        // If we're holding the grab button, and we're hovering a wall
        if (newState &&
            Physics.SphereCast(grabPoint.transform.position,
                grabPoint.transform.localScale.x / 2,
                Vector3.up,
                out rayHit,
                0.001f,
                grabbableLayers)) {
            smoothLocomotion.activeGrappler = this;
            smoothLocomotion.enabled = false;

            // TODO: Need to anchor grabPoint to a wall
            GrappleTo(parentHand.transform.position, grabPoint.transform.position);
        } else {
            smoothLocomotion.activeGrappler = this;
            smoothLocomotion.enabled = false;
        }
    }
}
