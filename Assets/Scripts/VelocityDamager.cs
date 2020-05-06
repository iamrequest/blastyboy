using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This damager inflicts damage if it collides with a damageable target, 
//  and is going faster than some threshold.
public class VelocityDamager : Damager {
    [Tooltip("The minimum velocity that this gameobject must be going to inflict damage, upon collision.")]
    public float damageVelocity;

    private Vector3 previousPosition;

    private void LateUpdate() {
        previousPosition = transform.position;
    }

    // TODO: play with triggers and freeze rot/pos.
    // Almost have it working, it's an issue with kinematic colliders
    private void OnTriggerEnter(Collider other) {
        Vector3 deltaVelocity = (transform.position - previousPosition) * Time.deltaTime;

        if(deltaVelocity.magnitude > damageVelocity) {

            // If the thing we're colliding with can take damage, then inflict it.
            Damagable damageable = other.GetComponent<Damagable>();
            if (damageable != null) {
                damageable.receiveDamage(damage);
            }
        }
        
    }
}
