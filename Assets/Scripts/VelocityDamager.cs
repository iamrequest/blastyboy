using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This damager inflicts damage if it collides with a damageable target, 
//  and is going faster than some threshold.
public class VelocityDamager : Damager {
    [Tooltip("The minimum velocity that this gameobject must be going to inflict damage, upon collision.")]
    public float damageVelocity;

    public bool playAudioOnDamage;
    public AudioSource audioSource;
    public AudioClip onDamageAudioClip;

    private Vector3 previousPosition;

    private void LateUpdate() {
        previousPosition = transform.position;
    }

    // Almost have it working, it's an issue with kinematic colliders
    private void OnTriggerEnter(Collider other) {
        Vector3 deltaVelocity = (transform.position - previousPosition) * Time.deltaTime;

        if(deltaVelocity.magnitude > damageVelocity) {

            // If the thing we're colliding with can take damage, then inflict it.
            Damagable damageable = other.GetComponent<Damagable>();
            if (damageable != null) {
                // Only play audio if the target is actually going to take damage
                if (!damageable.isInvincible && playAudioOnDamage) {
                    audioSource.PlayOneShot(onDamageAudioClip);
                }

                // Calculate damage
                damageable.receiveDamage(damage);
            }
        }
        
    }
}
