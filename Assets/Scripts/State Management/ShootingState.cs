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

    public float domHandDistanceFromEye;
    public Vector3 domHandRotationOffset;
    public Vector3 domHandPositionOffset;
    public Vector3 subHandRotationOffset;
    public Vector3 subHandPositionOffset;// Offset from dom hand

    [Header("Gunplay")]
    public float gunshotCooldown;
    public float initialGunshotDelay; // Delay that the agent must wait before firing their gun for the first time
    private float lastShotFired;

    public Transform gunBarrelTransform;
    public GameObject bulletPrefab;
    public float bulletSpeed;


    public override void OnStateEnter(BaseState previousState) {
        lastShotFired = Time.time - gunshotCooldown + initialGunshotDelay;
        domHandIK.weight = 1;
        subHandIK.weight = 1;
        headLookAtConstraint.weight = 1;

        parentFSM.animator.SetBool("isAiming", true);
    }

    public override void OnStateExit(BaseState previousState) {
        domHandIK.weight = 0;
        subHandIK.weight = 0;
        headLookAtConstraint.weight = 0;

        parentFSM.animator.SetBool("isAiming", false);
    }

    private void Update() {
        // Rotate to face the target, projected onto the x/z plane
        Vector3 dirToTarget = Vector3.ProjectOnPlane(target.transform.position - parentFSM.agentTransform.position, Vector3.up);
        parentFSM.agentTransform.rotation = Quaternion.LookRotation(dirToTarget);

        // Move the head rotation, and the IK arms
        MoveIKTargets();

        if (Time.time > lastShotFired + gunshotCooldown) {
            parentFSM.animator.SetTrigger("doGunshot");

            audioSource.PlayOneShot(gunshotAudioClip);

            // Instantiate the bullet, and give it forward velocity
            GameObject bullet = Instantiate(bulletPrefab, gunBarrelTransform.position, gunBarrelTransform.rotation);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            bulletRb.AddForce(gunBarrelTransform.transform.forward * bulletSpeed, ForceMode.Impulse);

            lastShotFired = Time.time;
        }
    }

    private void MoveIKTargets() {
        // Rotate the head to look at the target
        headLookAtConstraint.data.sourceObjects[0].transform.position = target.transform.position;

        Vector3 eyeToTargetDelta = target.transform.position - vision.eyePosition.position;
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
}
