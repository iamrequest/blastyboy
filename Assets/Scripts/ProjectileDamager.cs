using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDamager : Damager {
    public float lifespan;
    public bool selfDestructAfterLifespan;

    private void Start() {
        if (selfDestructAfterLifespan) {
            StartCoroutine(DestroySelfAfterDelay(lifespan));
        }
    }

    private void OnCollisionEnter(Collision collision) {
        // Minor cheat, since we're using non-damaging projectiles for force grab
        if (damage > 0) {
            // If the thing we're colliding with can take damage, then inflict it.
            Damagable damageable = collision.collider.GetComponent<Damagable>();
            if (damageable != null) {
                damageable.receiveDamage(damage);
            }
        }

        // Destroy the projectile after some delay
        DestroySelf();
    }

    private void OnTriggerEnter(Collider other) {
        // Minor cheat, since we're using non-damaging projectiles for force grab
        if (damage > 0) {
            // If the thing we're colliding with can take damage, then inflict it.
            Damagable damageable = other.GetComponent<Damagable>();
            if (damageable != null) {
                damageable.receiveDamage(damage);
            }
        }

        // Destroy the projectile after some delay
        DestroySelf();
    }

    private void DestroySelf() {
        Destroy(gameObject);
    }

    IEnumerator DestroySelfAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        DestroySelf();
    }
}
