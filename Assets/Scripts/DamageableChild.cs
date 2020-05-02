using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class will recieve damage, and pass it on to the parent gameobject
// This has usage for when we have a gameobject that has multiple child colliders.
//  Each collider should recieve damage, and report back to the main damageable parent.
public class DamageableChild : Damagable {
    public Damagable parentDamageable;

    // Whenever this child gameobject receives damage, pass it along the parent gameobject.
    //  It will be keeping track.
    public override void receiveDamage(int damage) {
        // Just in-case we want to keep track of damage for this child, process damage here.
        base.receiveDamage(damage);

        // Pass damage on to the parent as well.
        parentDamageable.receiveDamage(damage);
    }
}
