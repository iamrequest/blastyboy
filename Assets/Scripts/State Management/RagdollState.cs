using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollState : BaseState {
    public RagdollEnemy ragdollEnemy;
    public Damagable damageable;
    public ShootingState shootState;

    private bool isForceGrabbed;
    private float timeLastForceGrabbed;
    public float ragdollReleaseDelay;
    public float minDistanceFromGround;
    public LayerMask groundCheckLayerMask;
    public float getUpDelay;
    public float ragdollDisableDistance;

    public override void OnStateEnter(BaseState previousState) { 
        ragdollEnemy.isRagdollActive = true;
    }

    public override void OnStateExit(BaseState nextState) {
        ragdollEnemy.isRagdollActive = false;
        parentFSM.animator.SetTrigger("doGetUpFromBack");
    }
    public void OnGrab() {
        isForceGrabbed = true;
        timeLastForceGrabbed = Time.time;
    }

    public void OnRelease() {
        isForceGrabbed = false;
    }

    private void Update() {
        // Release the force grab after a duration of being released
        if (!isForceGrabbed) {
            // Make sure we're still alive
            if (damageable.currentHealth > 0) {
                // If we've been let go for a sufficient amount of time
                if (Time.time > timeLastForceGrabbed + ragdollReleaseDelay) {
                    // Make sure we're grounded
                    RaycastHit rayHit;

                    if (Physics.Raycast(ragdollEnemy.hips.transform.position, Vector3.up * -1, out rayHit, minDistanceFromGround, groundCheckLayerMask)) {
                        parentFSM.TransitionTo(shootState);
                        shootState.SpotTarget(shootState.target.transform.position);
                    }
                }
            } else {
                // If we're far from the player, set isKinematic to save frames
                ragdollEnemy.SetKinematic(vision.DistanceToTarget(parentFSM.target.transform.position) > ragdollDisableDistance);
            }
        }
    }
}
