using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Blaster : MonoBehaviour {
    private Hand parentHand;
    private BlasterGrappler grappler;
    public AudioSource audioSource;
    public MeshRenderer blasterMeshRenderer;

    [Header("General Firing")]
    public SteamVR_Action_Boolean fireAction;
    public Vector3 firingDirection;
    public Transform spawnTransform;

    [Header("Off-hand HandGuard")]
    public GameObject offHandHoldArea;
    public float offHandReleaseDistance;

    public AudioClip fireModeSwitchAudioClip, fireModeActivateFailClip;
    public Animator animator;
    public List<BlasterFireMode> fireModes; // This acts as a list of of swappable modules for the blaster
    private BlasterFireMode previousFireMode;
    private int primaryFireMode;
    private float lastFireModeSwitch;


    // TODO:
    // - Alt fire mode when the rear handle is held
    // - How to avoid triggering damage multiple times when multiple limbs are hit?
    void Start() {
        parentHand = GetComponentInParent<Hand>();
        grappler = GetComponent<BlasterGrappler>();
        audioSource = GetComponent<AudioSource>();

        // Primary hand SteamVR actions
        fireAction.AddOnStateDownListener(FireProjectile, parentHand.handType);
        fireAction.AddOnStateUpListener(PrimaryFireStateUp, parentHand.handType);
        //fireAction.AddOnUpdateListener(PrimaryFireStateUpdate, parentHand.otherHand.handType);

        // Off-hand SteamVR actions
        fireAction.AddOnStateDownListener(XR_SetNextPrimaryFireMode, parentHand.otherHand.handType);

        lastFireModeSwitch = Time.time;
        fireModes[0].OnFireModeSelected();
        UpdateBlasterGlowMaterial();
    }

    public GameObject FireNewProjectile(GameObject projectilePrefab, float projectileSpeed) {
        // TODO: Object pool these projectiles
        // Spawn a projectile at the specified position and rotation
        GameObject projectile = Instantiate(projectilePrefab, spawnTransform.position, spawnTransform.rotation);
        return FireExistingProjectile(projectile, projectileSpeed, true);
    }

    public GameObject FireExistingProjectile(GameObject projectile, float projectileSpeed, bool addTorque) {
        // Give the projectile forward force
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.AddRelativeForce(firingDirection * projectileSpeed, ForceMode.Impulse);

        // TODO: Add random rotation?
        if (addTorque) {
            rb.AddTorque(1, 0, 0);
        }

        return projectile;
    }

    // --------------------------------------------------------------------------------
    // SteamVR
    // --------------------------------------------------------------------------------
    public bool IsPrimaryFireModeBlockingGrapple() {
        return fireModes[primaryFireMode].isBlockingGrapple && IsTransitioningBetweenFireModes();
    }
    private void FireProjectile(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        // Can't shoot the gun when we're in the middle of force grabbing
        if (grappler.isGrappleProjectileDeployed) return;

        // Can't shoot the gun if we're in the middle of transitioning
        if (IsTransitioningBetweenFireModes()) {
            audioSource.PlayOneShot(fireModeActivateFailClip);
            return;
        }

        // Pass the fire action to the active "Blaster Module"
        fireModes[primaryFireMode].OnStateDown(fromAction, fromSource);
    }

    private void PrimaryFireStateUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        // Can't shoot the gun when we're in the middle of force grabbing
        if (grappler.isGrappleProjectileDeployed) return;
        if (IsTransitioningBetweenFireModes()) return;

        // Pass the fire action to the active "Blaster Module"
        fireModes[primaryFireMode].OnStateUp(fromAction, fromSource);
    }

    private void PrimaryFireStateUpdate(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
        // Can't shoot the gun when we're in the middle of force grabbing
        if (grappler.isGrappleProjectileDeployed) return;
        if (IsTransitioningBetweenFireModes()) return;

        // Pass the fire action to the active "Blaster Module"
        //fireModes[primaryFireMode].OnStateUpdate(fromAction, fromSource, newState);
    }


    // --------------------------------------------------------------------------------
    // Off-hand blaster handhold
    // --------------------------------------------------------------------------------
    // If the off-hand gets too far from the hand-hold area, we should detach the hand
    public void CheckOffHandDistance() {
        // Make sure we're actually holding on to the blaster
        if (parentHand.otherHand.currentAttachedObject == offHandHoldArea) {
            // Check the distance between the off-hand, and the hand-hold area
            Vector3 blasterOffHandPositionDelta = offHandHoldArea.transform.position - parentHand.otherHand.transform.position;

            if (blasterOffHandPositionDelta.magnitude > offHandReleaseDistance) {
                parentHand.otherHand.DetachObject(offHandHoldArea);
            }
        }
    }

    public void SetNextPrimaryFireMode() {
        lastFireModeSwitch = Time.time;

        // Transition out of the prev fire mode
        fireModes[primaryFireMode].OnFireModeDeselected();
        previousFireMode = fireModes[primaryFireMode];

        // Advance
        primaryFireMode += 1;
        primaryFireMode %= fireModes.Count;

        // Play an audio cue
        audioSource.PlayOneShot(fireModeSwitchAudioClip);

        // Update the material's color
        UpdateBlasterGlowMaterial();
        fireModes[primaryFireMode].OnFireModeSelected();
    }

    private bool IsTransitioningBetweenFireModes() {
        float transitionDuration = fireModes[primaryFireMode].transitionInDuration;
        if (previousFireMode != null) {
            transitionDuration += previousFireMode.transitionOutDuration;
        }

        return Time.time < lastFireModeSwitch + transitionDuration;
    }

    private void UpdateBlasterGlowMaterial() {
        // Setting a property in the shader
        // https://answers.unity.com/questions/1509757/shader-graph-edit-parameters-from-script.html
        blasterMeshRenderer.materials[1].SetColor("MainColor", fireModes[primaryFireMode].newMainColor);
    }

    private void XR_SetNextPrimaryFireMode(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        if (fromSource != parentHand.otherHand.handType) return;
        if (parentHand.otherHand.currentAttachedObject != offHandHoldArea) return;

        SetNextPrimaryFireMode();
    }
}
