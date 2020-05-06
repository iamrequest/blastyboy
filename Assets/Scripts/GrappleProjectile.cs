using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleProjectile : MonoBehaviour {
    public BlasterGrappler parentGrappler;

    private void OnTriggerEnter(Collider other) {
        // Register the collision
        if (!parentGrappler.isGrappling && !parentGrappler.isForceGrabbing) {
            parentGrappler.OnGrappleProjectileCollision(other);
        }
    }
}
