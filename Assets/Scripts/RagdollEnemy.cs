using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;


// BUG: Force grabbing a ragdoll's limb sometimes works.
//  Each limb is subject to the gravity of its parent limbs, and its grandparent limbs, etc.
//  So grabbing by a hand will make the rest of the RBs apply gravity by each of its limbs recursively
public class RagdollEnemy : MonoBehaviour {
    public bool isRagdollActiveOnStart;

    private Animator animator;
    public List<Rigidbody> limbs;
    private bool m_isRagdollActive;

    public bool isRagdollActive {
        get { return m_isRagdollActive;  }
        set {
            animator.enabled = !value;
            foreach (Rigidbody limbRigidbody in limbs) {
                limbRigidbody.isKinematic = !value;

                // Testing this post: https://answers.unity.com/questions/685219/move-a-specific-part-of-a-ragdoll-and-the-rest-of.html
                // Kinda works I think? Needs more work.
                if (value) {
                    limbRigidbody.drag = limbRigidbody.mass;
                } else {
                    limbRigidbody.drag = 0;
                }
            }

            m_isRagdollActive = value;
        }
    }

    // Start is called before the first frame update
    void Start() {
        animator = GetComponent<Animator>();
        isRagdollActive = isRagdollActiveOnStart;

        // -- Initialize our limbs
        // UnityEvents are null when the component is created via AddComponent()
        // Solution: https://forum.unity.com/threads/unity-event-is-null-right-after-addcomponent.819402/#post-5427855
        //
        // The unity namespace isn't available during builds however, so we need to wrap it in compiler flags
        // https://answers.unity.com/questions/576746/build-error-the-type-or-namespace-name-unityeditor.html
#if UNITY_EDITOR
        SerializedObject so;
#endif
        foreach (Rigidbody limbRigidbody in limbs) {
            DamageableChild dc = limbRigidbody.gameObject.AddComponent<DamageableChild>();
            dc.parentDamageable = GetComponent<Damagable>();

#if UNITY_EDITOR
            so = new SerializedObject(dc);
            so.Update();
#endif


            ForceGrabbableLimb fgl = limbRigidbody.gameObject.AddComponent<ForceGrabbableLimb>();
            fgl.ragdollParent = this;

#if UNITY_EDITOR
            so = new SerializedObject(fgl);
            so.Update();
#endif
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyUp(KeyCode.R)) {
            isRagdollActive = !isRagdollActive;
        }
    }
}
