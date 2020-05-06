using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

// This is a limb of a ragdollable enemy 
// When grabbed, the enemy's limb will go into a ragdoll state.
public class ForceGrabbableLimb : ForceGrabbable {
    public UnityEvent onRagdoll;
    public RagdollEnemy ragdollParent;
    public float originalDrag;

    private Rigidbody limbRigidbody;

    private Vector3 m_delegateTargetPickupPosition;
    public override Vector3 pickupPosition { 
        get {
            return m_delegateTargetPickupPosition;
        }
    }

    protected override void Start() {
        base.Start();
        limbRigidbody = rb;
    }

    public override void OnGrab(BlasterGrappler grappler) {
        // base.OnGrab(grappler);

        m_delegateTargetPickupPosition = rb.transform.position;
        ragdollParent.isRagdollActive = true;
    }

    public override void OnRelease(BlasterGrappler grappler) {
        base.OnRelease(grappler);
        ragdollParent.isRagdollActive = false;
    }


    public void OnRagdoll(bool isRagdollActive) {
        onRagdoll.Invoke();
    }

    public void ConfigureDelegateForceGrabTarget(GrappleProjectile delegateForceGrabTarget) {
        rb = delegateForceGrabTarget.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        rb.freezeRotation = true;

        CharacterJoint characterJoint = delegateForceGrabTarget.gameObject.AddComponent<CharacterJoint>();

        // Serialize the newly created component.
        // See RagdollEnemy for details
#if UNITY_EDITOR
        SerializedObject so;
        so = new SerializedObject(characterJoint);
        so.Update();
#endif

        characterJoint.connectedBody = limbRigidbody;
    }
}
