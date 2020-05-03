using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Blaster : MonoBehaviour {
    private Hand parentHand;
    private Grappler grappler;

    [Header("Firing")]
    public SteamVR_Action_Boolean fireAction;
    public GameObject projectilePrefab;
    public Transform spawnTransform;
    public float projectileSpeed;
    public Vector3 firingDirection;

    // TODO:
    // - Alt fire mode when the rear handle is held
    // - How to avoid triggering damage multiple times when multiple limbs are hit?
    void Start() {
        parentHand = GetComponentInParent<Hand>();
        grappler = GetComponent<Grappler>();

        fireAction.AddOnStateDownListener(FireProjectile, parentHand.handType);
    }

    private void FireProjectile(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        // Can't shoot the gun when we're in the middle of force grabbing
        if (grappler.IsGrappleDeployed()) return;

        FireProjectile(projectilePrefab, projectileSpeed);
    }

    public GameObject FireProjectile(GameObject projectilePrefab, float projectileSpeed) {
        // TODO: Object pool these projectiles
        // Spawn a projectile at the specified position and rotation
        GameObject projectile = Instantiate(projectilePrefab, spawnTransform.position, spawnTransform.rotation);

        // Give the projectile forward force
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.AddRelativeForce(firingDirection * projectileSpeed, ForceMode.Impulse);

        // TODO: Add random rotation?
        rb.AddTorque(1, 0, 0);

        return projectile;
    }
}
