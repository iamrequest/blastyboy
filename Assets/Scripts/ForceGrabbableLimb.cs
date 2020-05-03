using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This is a limb of a ragdollable enemy 
// When grabbed, the enemy's limb will go into a ragdoll state.
public class ForceGrabbableLimb : ForceGrabbable {
    public UnityEvent onRagdoll;
    public RagdollEnemy ragdollParent;
    public float originalDrag;

    public override void OnGrab(BlasterGrappler grappler) {
        base.OnGrab(grappler);
        rb.isKinematic = false;

        ragdollParent.isRagdollActive = true;
    }

    public override void OnRelease(BlasterGrappler grappler) {
        base.OnRelease(grappler);
        rb.isKinematic = false;
        ragdollParent.isRagdollActive = false;
    }


    public void OnRagdoll(bool isRagdollActive) {
        onRagdoll.Invoke();
    }
}
