using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ShootingState : BaseState {
    private const bool DEBUG = true;

    public AudioSource audioSource;
    public AudioClip gunshotAudioClip;

    public GameObject target;

    [Header("Animation Rigs")]
    public MultiAimConstraint headLookAtConstraint;
    public TwoBoneIKConstraint domHandIK, subHandIK;
    public GameObject domHandIKTarget, subHandIKTarget;
    public float animationRigActivateSpeed; // This determines how fast the animation rig weights will transition from 0 to 1 

    public float domHandDistanceFromEye;
    public Vector3 domHandRotationOffset;
    public Vector3 domHandPositionOffset;
    public Vector3 subHandRotationOffset;
    public Vector3 subHandPositionOffset;// Offset from dom hand

    [Header("Gunplay")]
    public float gunshotCooldown;
    public float initialGunshotDelay; // Delay that the agent must wait before firing their gun for the first time
    public float randomGunshotDelay, maxRandomGunshotDelay ; 
    private float lastShotFired;

    public Transform gunBarrelTransform;
    public GameObject bulletPrefab;
    public float bulletSpeed;

    [Header("State Management")]
    private float lastSeenTime;
    private Vector3 lastSeenPosition;
    public float giveUpDelay;
    public BaseState giveUpState;
    public float rotationSpeed;


    public override void OnStateEnter(BaseState previousState) {
        if (target == null) target = parentFSM.target;
        randomGunshotDelay = Random.Range(0, maxRandomGunshotDelay);

        lastShotFired = Time.time - gunshotCooldown + initialGunshotDelay + randomGunshotDelay;

        // If we just got up from a ragdoll state, give us enough time to stand up before shooting
        if (previousState == parentFSM.ragdollState) lastShotFired += parentFSM.ragdollState.getUpDelay;

        parentFSM.animator.SetBool("isAiming", true);
    }

    public override void OnStateExit(BaseState previousState) {
        domHandIK.weight = 0;
        subHandIK.weight = 0;
        headLookAtConstraint.weight = 0;

        parentFSM.animator.SetBool("isAiming", false);
    }

    private void Update() {
        // Ease our weights into an aiming pose
        if (domHandIK.weight < 1) {
            float addedWeight = Mathf.Min(animationRigActivateSpeed * Time.deltaTime, 1 - domHandIK.weight);

            domHandIK.weight += addedWeight;
            subHandIK.weight += addedWeight;
            headLookAtConstraint.weight += addedWeight;
        }

        // Update the last seen time/position of the player
        bool targetIsInSight = vision.IsInSight(target.transform.position, "Player");
        if (targetIsInSight) {
            lastSeenPosition = target.transform.position;
            lastSeenTime = Time.time;
        }

        // If it's been long enough since we've last seen the player, give up
        if (Time.time > lastSeenTime + giveUpDelay) {
            parentFSM.TransitionTo(giveUpState);
        }

        // Rotate to face the target, projected onto the x/z plane
        Vector3 dirToTarget = Vector3.ProjectOnPlane(lastSeenPosition - parentFSM.agentTransform.position, Vector3.up);
        parentFSM.agentTransform.rotation = Quaternion.Lerp(parentFSM.agentTransform.rotation, Quaternion.LookRotation(dirToTarget), rotationSpeed * Time.deltaTime);

        // Move the head rotation, and the IK arms
        MoveIKTargets();

        if (Time.time > lastShotFired + gunshotCooldown + randomGunshotDelay && targetIsInSight) {
            parentFSM.animator.SetTrigger("doGunshot");

            audioSource.PlayOneShot(gunshotAudioClip);

            // Instantiate the bullet, and give it forward velocity
            GameObject bullet = Instantiate(bulletPrefab, gunBarrelTransform.position, gunBarrelTransform.rotation);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            bulletRb.AddForce(gunBarrelTransform.transform.forward * bulletSpeed, ForceMode.Impulse);

            lastShotFired = Time.time;
            randomGunshotDelay = Random.Range(0, maxRandomGunshotDelay);
        }
    }

    private void MoveIKTargets() {
        // Rotate the head to look at the target
        headLookAtConstraint.data.sourceObjects[0].transform.position = lastSeenPosition;

        Vector3 eyeToTargetDelta = lastSeenPosition - vision.eyePosition.position;
        if (DEBUG) {
            Debug.DrawRay(gunBarrelTransform.position, eyeToTargetDelta, Color.red);
        }

        // Project a line from the eye to the target, with a magnituded of a specified distance from the eye
        //  (Plus our manually defined offset)
        Vector3 eyeToTargetOffset = (eyeToTargetDelta.normalized * domHandDistanceFromEye);

        // Move each hand target into position
        domHandIKTarget.transform.position = vision.eyePosition.position + eyeToTargetOffset + domHandPositionOffset;
        domHandIKTarget.transform.rotation = Quaternion.LookRotation(eyeToTargetDelta) * Quaternion.Euler(domHandRotationOffset);

        // Move the sub hand into position, relative to the dom hand
        //  For position offset, we're using a simple vertical offset (since the dom rot will change. We can't use a generic Vector3 to represent offset).
        //  Because the dom rot is changed, transform.forward actually represents "up"
        subHandIKTarget.transform.position = domHandIKTarget.transform.position 
            - (domHandIKTarget.transform.forward * subHandPositionOffset.z)
            - (domHandIKTarget.transform.up * subHandPositionOffset.y)
            - (domHandIKTarget.transform.right * subHandPositionOffset.x);
        
        subHandIKTarget.transform.rotation = domHandIKTarget.transform.rotation * Quaternion.Euler(subHandRotationOffset);
    }

    public void SpotTarget(Vector3 lastSeenPosition) {
        this.lastSeenPosition = lastSeenPosition;
        lastSeenTime = Time.time;
    }
}
