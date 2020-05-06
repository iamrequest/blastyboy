﻿using System.Collections;
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
            }

            m_isRagdollActive = value;
        }
    }

    // Start is called before the first frame update
    void Start() {
        animator = GetComponent<Animator>();
        isRagdollActive = isRagdollActiveOnStart;
    }
}
