using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollState : BaseState {
    public RagdollEnemy ragdollEnemy;
    public Damagable damageable;
    public BaseState nextStateAfterRagdoll;

    public override void OnStateEnter(BaseState previousState) { 
        ragdollEnemy.isRagdollActive = true;
    }

    public override void OnStateExit(BaseState nextState) {
        ragdollEnemy.isRagdollActive = false;
        parentFSM.animator.SetTrigger("doGetUpFromBack");
    }

    public void OnRelease() {
        if (damageable.currentHealth > 0) {
            parentFSM.TransitionTo(nextStateAfterRagdoll);
        }
    }
}
