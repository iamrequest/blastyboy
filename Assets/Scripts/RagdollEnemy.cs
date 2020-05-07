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

    public GameObject rootElement;
    public Rigidbody hips;
    private Quaternion ragdollEndRotation;
    private Vector3 ragdollEndPosition;

    public bool isRagdollActive {
        get { return m_isRagdollActive; }
        set {
            ragdollEndRotation = hips.transform.rotation;
            ragdollEndPosition = hips.transform.position;

            animator.enabled = !value;
            foreach (Rigidbody limbRigidbody in limbs) {
                limbRigidbody.isKinematic = !value;
            }

            // If we're transitioning out of ragdoll mode
            if (m_isRagdollActive && !value) {
                // TODO: This is putting the enemy in the sky on release
                //  Wait until collision with ground + delay?
                // TODO: Rotation should likely match too
                rootElement.transform.position = ragdollEndPosition;
            }

            m_isRagdollActive = value;
            //rootElement.transform.rotation = ragdollEndRotation;
        }
    }

    // Start is called before the first frame update
    void Start() {
        animator = GetComponent<Animator>();
        isRagdollActive = isRagdollActiveOnStart;
    }
}