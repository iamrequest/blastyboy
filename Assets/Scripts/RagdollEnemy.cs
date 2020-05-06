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

    [Header("States")]
    private bool isShootingAtPlayer;

    [Header("Animation")]
    [Tooltip("How long on average should the enemy wait before performing an idle flair animation? (ie: Kicking the ground)")]
    public float baseIdleAnimationTimeDelay;
    public float idleAnimationMaxTimeOffset;
    private float lastIdleAnimationTime, idleAnimationtimeDelayWithOffset;

    public bool isRagdollActive {
        get { return m_isRagdollActive; }
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

        CalculateIdleOffsetDelay();
    }

    private void Update() {
        if (!isRagdollActive) {
            if (isShootingAtPlayer) {
                OnPlayerEngagedUpdate();
            } else {
                // Idle animation flavor
                // After a delay, play one of two idle animations
                if (Time.time > lastIdleAnimationTime + idleAnimationtimeDelayWithOffset) {
                    // 50/50 chance of looking around, vs kicking the ground
                    // Could be cleaner/more robust, but this is a minor flavor addition, and I don't have a lot of time left.
                    if (Random.Range(0, 1) > .5) {
                        animator.SetTrigger("isIdleKicking");
                    } else {
                        animator.SetTrigger("isIdleLooking");
                    }

                    CalculateIdleOffsetDelay();
                }
            }
        }
    }

    // --------------------------------------------------------------------------------
    // Enemy AI
    // --------------------------------------------------------------------------------
    public void CalculateIdleOffsetDelay() {
        lastIdleAnimationTime = Time.time;
        idleAnimationtimeDelayWithOffset = lastIdleAnimationTime
                                           + baseIdleAnimationTimeDelay
                                           + Random.Range(-idleAnimationMaxTimeOffset, idleAnimationMaxTimeOffset);
    }

    private void OnPlayerEngage() {
        animator.SetBool("isShooting", true);
        isShootingAtPlayer = true;
    }
    private void OnPlayerDisengage() {
        animator.SetBool("isShooting", false);
        isShootingAtPlayer = false;
    }

    private void OnPlayerEngagedUpdate() { }
}