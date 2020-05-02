using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Blaster : MonoBehaviour {
    [Header("Firing")]
    public GameObject projectilePrefab;
    public Transform spawnTransform;
    public float projectileSpeed;
    public Vector3 firingDirection;

    [Header("SteamVR")]
    public SteamVR_Action_Boolean fireAction;
    private Hand parentHand;

    // Start is called before the first frame update
    void Start() {
        parentHand = GetComponentInParent<Hand>();
        fireAction.AddOnStateDownListener(fireProjectile, parentHand.handType);
    }

    private void fireProjectile(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        // TODO: Object pool these projectiles
        // Spawn a projectile at the specified position and rotation
        GameObject projectile = Instantiate(projectilePrefab, spawnTransform.position, spawnTransform.rotation);

        // Give the projectile forward force
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.AddRelativeForce(firingDirection * projectileSpeed, ForceMode.Impulse);
        rb.AddTorque(1, 0, 0);
    }
}
