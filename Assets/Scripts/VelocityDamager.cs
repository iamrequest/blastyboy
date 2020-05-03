using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This damager inflicts damage if it collides with a damageable target, 
//  and is going faster than some threshold.
public class VelocityDamager : Damager {
    [Tooltip("The minimum velocity that this gameobject must be going to inflict damage, upon collision.")]
    public float damageVelocity;

    private void OnCollisionEnter(Collision collision) {
        // TODO: This isn't working. The issue is related to colliders not triggering
        //Debug.Log(collision.collider.name);

        if (collision.relativeVelocity.magnitude > damageVelocity) {
            // If the thing we're colliding with can take damage, then inflict it.
            Damagable damageable = collision.collider.GetComponent<Damagable>();
            if (damageable != null) {
                damageable.receiveDamage(damage);
            }
        }
    }
}
