using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BaseState {
    public GameObject target;
    public ShootingState shootingState;

    [Tooltip("How long on average should the enemy wait before performing an idle flair animation? (ie: Kicking the ground)")]
    public float baseIdleAnimationTimeDelay;
    public float idleAnimationMaxTimeOffset;
    private float lastIdleAnimationTime, idleAnimationtimeDelayWithOffset;


    public override void OnStateEnter(BaseState previousState) {
        if (target == null) target = parentFSM.target;
        CalculateIdleOffsetDelay();
    }

    // Update is called once per frame
    void Update() {
        if (vision.IsInSight(target.transform.position, "Player")) {
            parentFSM.TransitionTo(shootingState);
        }

        // Idle animation flavor
        // After a delay, play one of two idle animations
        if (Time.time > lastIdleAnimationTime + idleAnimationtimeDelayWithOffset) {
            // 50/50 chance of looking around, vs kicking the ground
            // Could be cleaner/more robust, but this is a minor flavor addition, and I don't have a lot of time left.
            if (Random.Range(0, 1) > .5) {
                parentFSM.animator.SetTrigger("isIdleKicking");
            } else {
                parentFSM.animator.SetTrigger("isIdleLooking");
            }

            CalculateIdleOffsetDelay();
        }
    }
    public void CalculateIdleOffsetDelay() {
        lastIdleAnimationTime = Time.time;
        idleAnimationtimeDelayWithOffset = lastIdleAnimationTime
                                           + baseIdleAnimationTimeDelay
                                           + Random.Range(-idleAnimationMaxTimeOffset, idleAnimationMaxTimeOffset);
    }
}
