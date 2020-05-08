using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class TechDoor : MonoBehaviour {
    Animator animator;
    public Transform proximityTarget;
    public Vector3 doorCenterPosition;
    public float doorOpenDistance, doorCloseDistance;

    public bool isDoorOpenOnStart;
    private bool m_isDoorOpen;
    public bool isDoorOpen {
        get {
            return m_isDoorOpen;
        }
        set {
            if (isDoorLocked) return;
            animator.SetBool("isDoorOpen", !value);
            m_isDoorOpen = value;
        }
    }

    private bool m_isDoorLocked;
    public bool isDoorLocked {
        get {
            return m_isDoorLocked;
        }
        set {
            m_isDoorLocked = value;
            lockIcon.SetActive(value);
        }
    }
    public GameObject lockIcon;

    private void Start() {
        animator = GetComponent<Animator>();
        animator.StopPlayback();

        m_isDoorLocked = lockIcon.activeInHierarchy;
        m_isDoorOpen = isDoorOpenOnStart;
    }

    private void Update() {
        if (proximityTarget == null) {
            proximityTarget = Player.instance.hmdTransform;
        }

        if (!isDoorLocked) {
            if (proximityTarget != null) {
                float distanceToDoor = (proximityTarget.position - doorCenterPosition).magnitude;

                if (isDoorOpen) {
                    if (distanceToDoor > doorCloseDistance) {
                        isDoorOpen = false;
                    }
                } else {
                    if (distanceToDoor < doorOpenDistance) {
                        isDoorOpen = true;
                    }
                }
            }
        }
    }
}
