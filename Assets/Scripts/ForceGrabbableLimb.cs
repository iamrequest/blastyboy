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
    public FiniteStateMachine parentFSM;
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
        parentFSM.TransitionTo(parentFSM.ragdollState);
        parentFSM.ragdollState.OnGrab();
    }

    public override void OnRelease(BlasterGrappler grappler) {
        //base.OnRelease(grappler);
        parentFSM.ragdollState.OnRelease();
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
        // UnityEvents are null when the component is created via AddComponent()
        // Solution: https://forum.unity.com/threads/unity-event-is-null-right-after-addcomponent.819402/#post-5427855
        //
        // The unity namespace isn't available during builds however, so we need to wrap it in compiler flags
        // https://answers.unity.com/questions/576746/build-error-the-type-or-namespace-name-unityeditor.html
#if UNITY_EDITOR
        SerializedObject so;
        so = new SerializedObject(characterJoint);
        so.Update();
#endif

        characterJoint.connectedBody = limbRigidbody;
    }
}
