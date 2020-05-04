using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class BlasterGrappler : Grappler {
    private Blaster blaster;
    private LineRenderer lineRenderer;

    [Header("Grappling Projectile")]
    public GameObject forceGrabProjectilePrefab;
    public GameObject currentForceGrabProjectile;
    private GrapplePoint currentGrapplePoint;
    public float forceGrabProjectileVelocity;

    [Header("Grappling")]
    public float maxGrabDistance;

    [Header("Force Grab")]
    // Movement of the force grabbed object
    public float forceGrabSpeed;
    public float forceGrabRotationSpeed;
    public AnimationCurve forceGrabDampening;

    // How far away from the blaster should the target float?
    public float initialForceGrabDistance;
    public float minForceGrabDistance;
    private float currentForceGrabDistance;

    // The target we're holding on to
    private ForceGrabbable forceGrabbableTarget;

    [Header("Post-Force-Grab Throw")]
    public Vector3 blasterVelocity;
    public float forceGrabThrowVelocity;

    [Header("Force Grab (Relative Push/Pull)")]
    public SteamVR_Action_Boolean relativePushPullAction;
    public bool relativePushPull;
    public float pushPullVelocity;
    public Vector3 forceGrabBlasterStartPosition;

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        blaster = GetComponent<Blaster>();
        lineRenderer = GetComponent<LineRenderer>();

        //drawGrabAction.AddOnUpdateListener(DrawGrab, parentHand.handType);
        doGrabAction.AddOnUpdateListener(DoGrab, parentHand.handType);
        relativePushPullAction.AddOnUpdateListener(SetRelativePushPullMode, parentHand.handType);
    }


    protected override void Update() {
        if (characterController.isGrounded) {
            motion = Vector3.zero;
        }
    }

    // Are we currently in the middle of pulling ourselves via the grapple?
    public override bool IsCurrentlyGrappling() {
        // We must have a grapple point deployed (and collided)
        if (currentGrapplePoint == null) return false;

        // We must be holding the grapple button
        // We must not be in the middle of a force grab
        return (doGrabAction.trackedDeviceIndex == parentHand.GetDeviceIndex())
            && doGrabAction.state
            && !currentGrapplePoint.hasCollided
            && (forceGrabbableTarget == null);
    }

    // Are we currently using the blaster to fire a grapple?
    public override bool IsGrappleDeployed() {
        return (doGrabAction.trackedDeviceIndex == parentHand.GetDeviceIndex()) && doGrabAction.state;
    }

    // --------------------------------------------------------------------------------
    // New Grapple
    // --------------------------------------------------------------------------------
    // TODO: Apply motion += grapplePoint.velocity?
    private void DoGrab(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
        // TODO: This is causing the player to freeze in place if they activate DoGrab() with their off hand
        //if (doGrabAction.trackedDeviceIndex != parentHand.GetDeviceIndex()) return;

        // If the player lets go of the grab button
        if (!newState) {
            // Let go of the force grab, if one's happening
            if (forceGrabbableTarget != null) {
                forceGrabbableTarget.OnRelease(this);
                forceGrabbableTarget = null;
            }

            DestroyCurrentGrapplePoint();

            // If we have vertical momentum on the way up, give ourselves a little extra boost
            if (fromAction.stateUp && motion.y > 0) {
                motion.y += addedVerticalMomentum;
            }
            smoothLocomotion.enabled = true;
            return;
        }

        // If this is the first time that we've pressed the grab button
        if (fromAction.stateDown) {
            // If we haven't connected with anything yet, fire a projectile
            if (forceGrabbableTarget == null) {
                DestroyCurrentGrapplePoint();

                currentForceGrabProjectile = blaster.FireProjectile(forceGrabProjectilePrefab, 
                    forceGrabProjectileVelocity);

                currentGrapplePoint = currentForceGrabProjectile.GetComponent<GrapplePoint>();
                currentGrapplePoint.parentGrappler = this;
            }
        }

        // When the grapple point collided with the target...
        if (newState && currentForceGrabProjectile != null) {
            if (currentGrapplePoint.hasCollided) {
                if (forceGrabbableTarget != null) {
                    // -- This is a force-grabbable object, then pick it up
                    DoForceGrabDistance();

                    // Not sure why this is needed, but smooth locomotion gets disabled without it.
                    smoothLocomotion.enabled = true;
                } else {
                    // -- If we've collided with a wall, then grapple to it.
                    smoothLocomotion.activeGrappler = this;
                    smoothLocomotion.enabled = false;

                    GrappleTo(blaster.spawnTransform.position, currentForceGrabProjectile.transform.position);
                }
            }
        } else {
            smoothLocomotion.enabled = true;
        }
    }

    private void DestroyCurrentGrapplePoint() {
        if (currentForceGrabProjectile != null) {
            // Trash the grapple point if we've hit a wall.
            // If we haven't hit a wall yet, then it'll garbage collect by itself after its lifespan is up.
            GrapplePoint gp = currentForceGrabProjectile.GetComponent<GrapplePoint>();
            if (gp.hasCollided) {
                gp.DestroySelf(); 
            }
        }
    }

    // --------------------------------------------------------------------------------
    // New Force Grab
    // --------------------------------------------------------------------------------
    // x Force grab
    // TODO: Relative pull/push
    //  - Absolute mode: 
    //      - Force grab position is lerp'd to a point x meters from the blaster hand
    //  - Relative mode:
    //      - Trigger button activates pull/push mode
    //      - Horizontal/vertical motion is the same
    //      - Forward/back is relative to blaster's position when relative mode was entered
    //
    // TODO: Throw velocity to match Throwable
    public void RegisterForceGrabbable(GameObject target) {
        forceGrabBlasterStartPosition = blaster.spawnTransform.position;
        relativePushPull = false;

        forceGrabbableTarget = target.GetComponentInParent<ForceGrabbable>();
        if (forceGrabbableTarget != null) {
            currentForceGrabDistance = initialForceGrabDistance;
            forceGrabbableTarget.OnGrab(this);
        }
    }

    public void DoForceGrabDistance() {
        // Find the point starting from the blaster, $forceGrabDistance meters away.
        //  The force grabbable object will hover there.
        Vector3 floatPosition = blaster.spawnTransform.position + transform.forward * currentForceGrabDistance;

        // If we're in Relative Push/Pull mode, update the float distance.
        // The distance is updated relative to how far the blaster is forward/back compared to when we pressed the button.
        if (relativePushPull) {
            // Find the vector from the blaster to the float pos
            //  We'll need to project onto this line later
            Vector3 blasterFloatPositionDelta = blaster.spawnTransform.position - floatPosition;

            // Find the vector from the blaster's start and current position
            Vector3 blasterPositionDelta = forceGrabBlasterStartPosition - blaster.spawnTransform.position;

            // Project the second line onto the first
            Vector3 pullPushDelta = Vector3.Project(blasterPositionDelta, blasterFloatPositionDelta.normalized);


            // Add on some forward/backwards momentum 
            // The if statement is a hack to tell if the player is moving the blaster forward or backwards.
            //  There's probably a better way of testing this, but that's a code jam for you~
            if (pullPushDelta.normalized == blasterFloatPositionDelta.normalized) {
                currentForceGrabDistance += pullPushDelta.magnitude * pushPullVelocity * Time.deltaTime;
            } else {
                currentForceGrabDistance -= pullPushDelta.magnitude * pushPullVelocity * Time.deltaTime;
            }

            if (currentForceGrabDistance < minForceGrabDistance) {
                currentForceGrabDistance = minForceGrabDistance;
            }

        }

        // Find the dir from the target to the float position
        Vector3 floatPositionDelta = floatPosition - forceGrabbableTarget.transform.position;

        // Float the object towards the float position
        forceGrabbableTarget.transform.position = Vector3.MoveTowards(forceGrabbableTarget.transform.position,
                                                                floatPosition,
                                                                forceGrabSpeed 
                                                                    * forceGrabDampening.Evaluate(floatPositionDelta.magnitude)
                                                                    * Time.deltaTime);

        // Rotate the object to match the blaster's rotation
        forceGrabbableTarget.transform.rotation = Quaternion.Lerp(forceGrabbableTarget.transform.rotation,
                                                            blaster.spawnTransform.rotation,
                                                            forceGrabRotationSpeed * Time.deltaTime);
    }

    private void SetRelativePushPullMode(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
        if (newState == relativePushPull) return;

        forceGrabBlasterStartPosition = blaster.spawnTransform.position;
        relativePushPull = newState;
    }

    // Call this after releasing the ForceGrabbable object
    public void ApplyForceGrabThrow() {
        // Apply a throwing force, 
        forceGrabbableTarget.rb.AddForce(blasterVelocity * forceGrabThrowVelocity, ForceMode.Impulse);

        // -- Apply the leftover velocity from the grab
        // Find the dir from the target to the float position
        Vector3 floatPosition = blaster.spawnTransform.position + transform.forward * currentForceGrabDistance;
        Vector3 floatPositionDelta = floatPosition - forceGrabbableTarget.transform.position;
    }


























    // --------------------------------------------------------------------------------
    // Deprecated/old grapple logic
    // --------------------------------------------------------------------------------
    /*
    private void DrawGrab(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
        if (newStatje) {
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
                hoveredObject = rayHit.collider.gameObject;
                grabPoint.SetActive(true);

                grabPoint.transform.position = rayHit.point;
            } else {
                hoveredObject = null;
                grabPoint.SetActive(false);
            }
        } else {
            lineRenderer.enabled = false;
            hoveredObject = null;
            grabPoint.SetActive(false);
        }
    }

    // TODO: Only grapple to certain gameobjects
    // TODO: Add force grab
    private void DoGrabOld(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
        // If we're in the middle of pulling ourselves towards the grapple point, disable smooth locomotion.
        if (newState && grabPoint.activeInHierarchy) {
            // TODO: Cache this for performance
            forceGrabbable = hoveredObject.GetComponent<ForceGrabbable>();

            if (forceGrabbable) {
                //debug.log(fromaction.laststate + " " + forcegrabbable.isgrabbed);
                // -- Pick up the target
                // On the first frame that we grab...
                if (!fromAction.lastState && !forceGrabbable.isGrabbed) {
                    forceGrabStartPosition = blaster.spawnTransform.position;
                    forceGrabbable.OnGrab(this);
                }

                DoForceGrabRb();
            } else {
                // -- Grapple to the target
                smoothLocomotion.enabled = false;
                GrappleTo(blaster.spawnTransform.position, grabPoint.transform.position);
            }
        } else {
            // On the first frame that we release...
            if (forceGrabbable != null && fromAction.lastStateUp) {
                forceGrabbable.OnRelease(this);
                forceGrabbable = null;
            }

            smoothLocomotion.enabled = true;
        }
    }

    // --------------------------------------------------------------------------------
    // Deprecated/Old Force grab
    // --------------------------------------------------------------------------------
    public void DoForceGrabVelocity() {
        // The change in the blaster's position from the start of the force grab, and now.
        Vector3 blasterPositionDelta =  blaster.spawnTransform.position - forceGrabStartPosition;

        // TODO: Lerp with AnimationCurve?
        forceGrabbable.transform.position = forceGrabbable.pickupPosition
                                            + blasterPositionDelta;
                      

        forceGrabbable.transform.rotation = Quaternion.Lerp(forceGrabbable.transform.rotation,
                                                            blaster.spawnTransform.rotation,
                                                            forceGrabRotationSpeed);
    }

    public void DoForceGrabRb() {
        // The change in the blaster's position from the start of the force grab, and now.
        Vector3 blasterPositionDelta = blaster.spawnTransform.position - forceGrabStartPosition;

        forceGrabbable.rb.AddForce(blasterPositionDelta 
                                    * forceGrabSpeed 
                                    * forceGrabDampening.Evaluate(blasterPositionDelta.magnitude)
                                    * Time.deltaTime, 
                                    ForceMode.Impulse);
    }
    */
}
