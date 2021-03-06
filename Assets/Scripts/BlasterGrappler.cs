﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class BlasterGrappler : Grappler {
    Blaster blaster;
    public GameObject blasterMesh;

    [Header("Blaster Grapple")]
    public GameObject grappleProjectilePrefab;
    private GrappleProjectile grappleProjectile;
    public float grappleProjectileVelocity;

    public bool isGrappleProjectileDeployed;

    [Header("Force Grab")]
    public SteamVR_Action_Boolean relativePushPullAction;
    public bool isForceGrabbing;
    public float forceGrabSpeed, forceGrabRotationSpeed, pushPullSpeed;
    public float initialForceGrabDistance;
    private float currentForceGrabDistance;
    public AnimationCurve forceGrabDampening;

    // -- Relative push/pull
    private Vector3 forceGrabBlasterStartPosition;
    private bool relativePushPull;
    private ForceGrabbable forceGrabbableTarget;
    private Vector3 floatPosition;
    private float minForceGrabDistance = 1;

    [Header("Force Grab Line Renderer")]
    public LineRenderer mainLineRenderer;
    //public LineRenderer subLineRenderer;
    //public GameObject tempLineRendererTarget;


    protected override void Start() {
        base.Start();
        blaster = GetComponent<Blaster>();

        StopDrawingGrappleLighting();

        doGrabAction.AddOnUpdateListener(DoGrab, parentHand.handType);
        relativePushPullAction.AddOnUpdateListener(SetRelativePushPullMode, parentHand.handType);
    }
    protected override void Update() {
        base.Update();
    }

    private void InstantiateNewGrappleProjectile() {
        GameObject projectile = Instantiate(grappleProjectilePrefab);
        grabPoint = projectile.GetComponent<Rigidbody>();
        grappleProjectile = projectile.GetComponent<GrappleProjectile>();
        grappleProjectile.parentGrappler = this;
    }

    private void ReleaseGrip() {
        isGrappleProjectileDeployed = false;
        StopDrawingGrappleLighting();

        // If we're either in the middle of grappling, or shooting a grapple projectile
        if (grabPoint != null) {
            Destroy(grabPoint.gameObject);
        }

        if (isGrappling) {
            OnGrappleEnd();
        }

        if (isForceGrabbing) {
            OnForceGrabEnd();
            isForceGrabbing = false;
        }
    }

    private void DoGrab(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
        //if (doGrabAction.trackedDeviceIndex != parentHand.GetDeviceIndex()) return;
        if (blaster.IsPrimaryFireModeBlockingGrapple()) {
            ReleaseGrip();
        }

        if (newState) {
            DrawGrappleLightning();
        } else {
            ReleaseGrip();
        }

        if (fromAction.stateDown) {
            if (grabPoint == null) InstantiateNewGrappleProjectile();

            isGrappleProjectileDeployed = true;

            // Reset position/momentum
            grabPoint.transform.position = blaster.spawnTransform.position;
            grabPoint.velocity = Vector3.zero;

            grabPoint.gameObject.SetActive(true);
            grabPoint.AddRelativeForce(blasterMesh.transform.forward * grappleProjectileVelocity, ForceMode.Impulse);
        }

        if (newState) {
            if (isGrappling) {
                GrappleTo(blaster.spawnTransform.position, grabPoint.position);
            } else if (isForceGrabbing) {
                // If the target dissapeared on us, let go
                if (forceGrabbableTarget == null) {
                    ReleaseGrip();
                } else {
                    DoForceGrabDistance();
                }
            }
        }
    }


    public void OnGrappleProjectileCollision(Collider other) {
        // Test if we can actually collide with this object
        if (((1 << other.gameObject.layer) & grabbableLayers) == 0) {
            isGrappleProjectileDeployed = false;
            StopDrawingGrappleLighting();
            Destroy(grabPoint.gameObject);
            return;
        }


        // Test if we should grapple to the target, or force grab the target
        forceGrabbableTarget = other.GetComponent<ForceGrabbable>();
        if (forceGrabbableTarget == null) {
            // -- Grapple
            grabPoint.velocity = Vector3.zero;
            grabPoint.isKinematic = true;
            grabPoint.transform.parent = other.gameObject.transform;

            OnGrappleStart();
        } else {
            // -- Force grab
            ForceGrabbableLimb limb = forceGrabbableTarget as ForceGrabbableLimb;
            if (limb) {
                // Configure the character joint to stick to the target
                limb.ConfigureDelegateForceGrabTarget(grappleProjectile);

                limb.OnGrab(this);
            } else {
                // Stick to the target
                grabPoint.velocity = Vector3.zero;
                grabPoint.isKinematic = true;
                grabPoint.transform.parent = other.gameObject.transform;
            }

            OnForceGrabStart();
        }
    }

    protected override void OnGrappleStart() {
        base.OnGrappleStart();

        smoothLocomotion.enabled = false;
        smoothLocomotion.activeGrappler = this;
    }

    protected override void OnGrappleEnd() {
        base.OnGrappleEnd();
        isGrappleProjectileDeployed = false;

        smoothLocomotion.enabled = true;

        StopDrawingGrappleLighting();
        if (grabPoint != null) {
            Destroy(grabPoint.gameObject);
        }
    }

    // --------------------------------------------------------------------------------
    // Force grab
    // --------------------------------------------------------------------------------
    private void OnForceGrabStart() {
        isForceGrabbing = true;
        forceGrabBlasterStartPosition = blaster.spawnTransform.position;
        relativePushPull = false;

        if (forceGrabbableTarget != null) {
            currentForceGrabDistance = initialForceGrabDistance;
            forceGrabbableTarget.OnGrab(this);
        }
    }

    private void OnForceGrabEnd() {
        isGrappleProjectileDeployed = false;

        forceGrabbableTarget.OnRelease(this);

        StopDrawingGrappleLighting();
        if (grabPoint != null) {
            Destroy(grabPoint.gameObject);
        }
    }

    public void DoForceGrabDistance() {
        // Find the point starting from the blaster, $forceGrabDistance meters away.
        //  The force grabbable object will hover there.
        Vector3 floatPosition = blaster.spawnTransform.position + blasterMesh.transform.forward * currentForceGrabDistance;

        RecalculateForceGrabDistance();
        
        // Find the dir from the target to the float position
        Vector3 floatPositionDelta = forceGrabbableTarget.transform.position - floatPosition;

        // Float the object towards the float position
        forceGrabbableTarget.rb.transform.position = Vector3.MoveTowards(forceGrabbableTarget.rb.transform.position,
                                                                floatPosition,
                                                                forceGrabSpeed 
                                                                    * forceGrabDampening.Evaluate(floatPositionDelta.magnitude)
                                                                    * Time.deltaTime);

        // Rotate the object to match the blaster's rotation
        forceGrabbableTarget.rb.transform.rotation = Quaternion.Lerp(forceGrabbableTarget.rb.transform.rotation,
                                                            blaster.spawnTransform.rotation,
                                                            forceGrabRotationSpeed * Time.deltaTime);
    }

    private void RecalculateForceGrabDistance() {
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
                currentForceGrabDistance += pullPushDelta.magnitude * pushPullSpeed * Time.deltaTime;
            } else {
                currentForceGrabDistance -= pullPushDelta.magnitude * pushPullSpeed * Time.deltaTime;
            }

            if (currentForceGrabDistance < minForceGrabDistance) {
                currentForceGrabDistance = minForceGrabDistance;
            }
        }
    }
    private void SetRelativePushPullMode(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
        if (newState == relativePushPull) return;

        forceGrabBlasterStartPosition = blaster.spawnTransform.position;
        relativePushPull = newState;
    }

    // --------------------------------------------------------------------------------
    // Force grab lightning
    // --------------------------------------------------------------------------------
    public void StopDrawingGrappleLighting() {
        mainLineRenderer.enabled = false;
    }

    // Would be nice: Multiple line renderers acting like lighting with a random offset from the main beam
    public void DrawGrappleLightning() {
        if (grappleProjectile != null) {
            // -- Draw the main beam
            mainLineRenderer.enabled = true;
            mainLineRenderer.SetPosition(0, blaster.spawnTransform.position);
            mainLineRenderer.SetPosition(1, grappleProjectile.transform.position);
        }


        // -- Render sub line renderers
        //Vector3 lineRendererDelta = tempLineRendererTarget.transform.position - blaster.spawnTransform.position;

        //subLineRenderer.SetPosition(0, blaster.spawnTransform.position);
        //for (float segmentNum = 1f; segmentNum < subLineRenderer.positionCount; segmentNum++) {
        //    float div = (segmentNum + 1) / (subLineRenderer.positionCount);
        //    Vector3 pos = blaster.spawnTransform.position + (lineRendererDelta * div);

        //    subLineRenderer.SetPosition(Mathf.FloorToInt(segmentNum), pos);
        //}
    }
}
