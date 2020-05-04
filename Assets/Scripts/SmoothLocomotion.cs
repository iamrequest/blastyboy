using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SmoothLocomotion : MonoBehaviour
{
    public bool DRAW_DEBUG_RAYS;
    public Grappler activeGrappler;

    [Header("VR")]
    [Tooltip("The SteamVR input source that we should consider when doing transform.forward for player locomotion")]
    public SteamVR_Input_Sources forwardDirectionSource;
    private Transform forwardDirectionTransform;

    [Header("Locomotion")]
    public CharacterController characterController;
    public float speed;
    public SteamVR_Action_Vector2 movementAction;
    public float gravity;


    // -- Used in calculating playerm motion --
    private Vector3 motion;

    // Start is called before the first frame update
    void Start() {
        updateForwardDirectionTransform();
        movementAction.AddOnAxisListener(move, SteamVR_Input_Sources.Any);
    }

    private void move(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
        // Figure out the player's holding down
        motion += forwardDirectionTransform.transform.right * axis.x * speed;
        motion += forwardDirectionTransform.transform.forward * axis.y * speed;

        // Restrict movement to horizontal movement
        motion = Vector3.ProjectOnPlane(motion, Vector3.up);

        // Don't tie movement speed to framerate
        motion *= Time.deltaTime;
    }

    // Update is called once per frame
    void Update() {
        if (DRAW_DEBUG_RAYS) {
            Debug.DrawRay(forwardDirectionTransform.position, forwardDirectionTransform.forward, Color.yellow);
            Debug.DrawRay(forwardDirectionTransform.position, forwardDirectionTransform.right, Color.red);
            Debug.DrawRay(forwardDirectionTransform.position, motion * speed, Color.green);
        }
    }

    private void FixedUpdate() {
        // The character controller's bounding box reaches from the floor, to the player's eye position
        characterController.height = Player.instance.eyeHeight;
        characterController.center = new Vector3(Player.instance.hmdTransform.localPosition.x, 
                                                 Player.instance.hmdTransform.localPosition.y / 2,
                                                 Player.instance.hmdTransform.localPosition.z);

        // Apply gravity
        motion.y -= gravity * Time.deltaTime;

        // Apply conserved momentum from the grappler
        // Also, dampen the conserved momentum by gravity
        if (activeGrappler != null) {
            motion += activeGrappler.getLeftoverMomentum() * Time.deltaTime;
            activeGrappler.ApplyGravity(gravity);
        }

        // Perform the actual motion
        characterController.Move(motion);

        // Reset the motion for the next frame
        motion = Vector3.zero;

        if (Input.GetKeyUp(KeyCode.G)) {
            // Make sure we're pointing at the right direction
            updateForwardDirectionTransform();
        }
    }

    // Whenever we change the SteamVR_Input_Source that represents the forward direction, we want to update forwardDirectionTransform to point to the right transform.
    //  eg: When we change forwardDirectionSource to "left hand", we want to set forwardDirectionTransform to the transform of the left hand.
    void updateForwardDirectionTransform() {
        // Headset
        if (forwardDirectionSource == SteamVR_Input_Sources.Head) {
            forwardDirectionTransform = Player.instance.hmdTransform;
            return;
        }

        // Loop through our hands, and attempt to assign the transform to one of the hands
        foreach (Hand h in Player.instance.hands) {
            if (h.handType.Equals(forwardDirectionSource)) {
                forwardDirectionTransform = h.transform;
                return;
            }
        }

        // No match. Throw an error, since we don't have a proper transform
        Debug.LogError("No valid forward direction supplied");
    }
}
