using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour {
    public BlasterGrappler parentGrappler;
    public Rigidbody rb;
    public float spawnTimestamp, lifespan;
    public bool hasCollided;

    private void Start() {
        hasCollided = false;
        StartCoroutine(DestroySelfAfterDelay(lifespan));
    }

    private void OnTriggerEnter(Collider other) {
        // Register the collision
        //if (!hasCollided) {
            hasCollided = true;
            parentGrappler.RegisterForceGrabbable(gameObject);

            // Stick to the target
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            transform.parent = other.gameObject.transform;
        //}
    }

    public IEnumerator DestroySelfAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        // If the player has fired another grapple point, we can safely destroy ourselves.
        if (parentGrappler.currentForceGrabProjectile != this.gameObject) {
            DestroySelf();
        }

        // If we've collided with a wall, we shouldn't destroy ourselves anymore.
        if (!hasCollided) {
            DestroySelf();
        }
    }
    public void DestroySelf() {
        Destroy(gameObject);
    }
}
