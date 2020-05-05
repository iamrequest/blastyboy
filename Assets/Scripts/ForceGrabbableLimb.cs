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

    public List<ForceGrabbableLimb> parentLimbs;

    protected override void Start() {
        base.Start();
        FindParentLimbs();
    }

    public override void OnGrab(Grappler grappler) {
        base.OnGrab(grappler);
        rb.isKinematic = false;

        ragdollParent.isRagdollActive = true;
    }

    public override void OnRelease(Grappler grappler) {
        base.OnRelease(grappler);
        rb.isKinematic = false;
        ragdollParent.isRagdollActive = false;
    }


    public void OnRagdoll(bool isRagdollActive) {
        onRagdoll.Invoke();
    }

    private void FindParentLimbs() {
        parentLimbs = new List<ForceGrabbableLimb>();

        // Starting from this gameobject, loop through each parent component of the enemy
        // If there's a force grabbable limb, record it.
        GameObject childGameObject = gameObject;
        while (childGameObject != ragdollParent.gameObject && childGameObject != null) {
            childGameObject = childGameObject.transform.parent.gameObject;

            ForceGrabbableLimb limb = childGameObject.GetComponent<ForceGrabbableLimb>();
            if (limb != null) {
                // TEST
                parentLimbs.Clear();

                parentLimbs.Add(limb);
            }
        }
    }
}
