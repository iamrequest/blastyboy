using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class RotateToFaceVRPlayer : MonoBehaviour {
    // Typically, we only want to rotate along the Y axis.
    public bool constrainToYAxis;

    void Update() {
        if (constrainToYAxis) {
            transform.LookAt(Player.instance.hmdTransform.position);
        } else {
            // Figure out the direction from us to the player, and constrain that to the Y axis.
            transform.forward =
                Vector3.ProjectOnPlane(transform.position - Player.instance.hmdTransform.position,
                                       Vector3.up).normalized;
        }
    }
}
