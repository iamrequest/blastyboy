using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public abstract class Grappler : MonoBehaviour {
    protected Hand parentHand;
    protected Grappler otherGrappler;
    public Vector3 motion; // The current momentum applied by the grappler

    [Header("Config")]
    public CharacterController characterController;
    public SteamVR_Action_Boolean doGrabAction;
    public Rigidbody grabPoint;
    protected GameObject hoveredObject; // The object we're pointing the grappler at
    public SmoothLocomotion smoothLocomotion;

    [Header("Grappling")]
    public LayerMask grabbableLayers;
    public float pullSpeed;
    public AnimationCurve grappleDistanceDampening;
    public bool isGrappling;

    [Tooltip("When the user lets go of a grapple, should they maintain their momentum?")]
    public bool conserveMomentumOnGrappleStop;

    [Tooltip("When the user lets go of a grapple, how much extra vertical boost should they get?")]
    public float addedVerticalMomentum;

    // Start is called before the first frame update
    protected virtual void Start() {
        parentHand = GetComponentInParent<Hand>(); 
        otherGrappler = parentHand.otherHand.GetComponentInChildren<Grappler>();
    }

    protected virtual void Update() {
        if (characterController.isGrounded) {
            motion = Vector3.zero;
        }
    }
    protected virtual void OnGrappleStart() {
        isGrappling = true;

        // Conserve momentum between grapplers
        if (otherGrappler != null) {
            if (otherGrappler.isActiveAndEnabled) {
                if (otherGrappler.isGrappling) {
                    motion = otherGrappler.motion;
                    otherGrappler.OnGrappleEnd();
                }
            }
        }
    }

    protected virtual void OnGrappleEnd() { 
        isGrappling = false;

        if (conserveMomentumOnGrappleStop && motion.y > 0) {
            motion.y += addedVerticalMomentum;
        }
    }

    // --------------------------------------------------------------------------------
    // Grapple 
    // --------------------------------------------------------------------------------
    public void GrappleTo(Vector3 fromPosition, Vector3 targetPosition) {
        Vector3 dirToTarget = targetPosition - fromPosition;

        motion = dirToTarget.normalized
            * pullSpeed
            * grappleDistanceDampening.Evaluate(dirToTarget.magnitude);

        characterController.Move(motion
            * Time.deltaTime) ;
    }

    virtual public bool IsCurrentlyGrappling() { return false; }
    virtual public bool IsGrappleDeployed() { return false; }

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
