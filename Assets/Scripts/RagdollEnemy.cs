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

    public List<Rigidbody> limbs;
    private bool m_isRagdollActive;

    public bool isRagdollActive {
        get { return m_isRagdollActive;  }
        set {
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
        isRagdollActive = isRagdollActiveOnStart;

        // -- Initialize our limbs
        // UnityEvents are null when the component is created via AddComponent()
        // Solution: https://forum.unity.com/threads/unity-event-is-null-right-after-addcomponent.819402/#post-5427855
        SerializedObject so;
        foreach (Rigidbody limbRigidbody in limbs) {
            DamageableChild dc = limbRigidbody.gameObject.AddComponent<DamageableChild>();
            dc.parentDamageable = GetComponent<Damagable>();

            so = new SerializedObject(dc);
            so.Update();


            ForceGrabbableLimb fgl = limbRigidbody.gameObject.AddComponent<ForceGrabbableLimb>();
            fgl.ragdollParent = this;

            so = new SerializedObject(fgl);
            so.Update();
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyUp(KeyCode.R)) {
            isRagdollActive = !isRagdollActive;
        }
    }
}
