using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ForceGrabbable : MonoBehaviour {
    public UnityEvent onGrab, onRelease, onGrabUpdate;
    public bool isGrabbed;
    public Rigidbody rb;

    // The position where we initialized the grab
    public Vector3 m_pickupPosition;
    public Vector3 pickupPosition { 
        get {
            return m_pickupPosition;
        }
    }


    // Start is called before the first frame update
    protected virtual void Start() {
        isGrabbed = false;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        if (isGrabbed) {
            onGrabUpdate.Invoke();
        }
    }

    public virtual void OnGrab(BlasterGrappler grappler) {
        isGrabbed = true;
        m_pickupPosition = transform.position;
        rb.useGravity = false;

        onGrab.Invoke();
    }
    public virtual void OnRelease(BlasterGrappler grappler) {
        isGrabbed = false;
        rb.useGravity = true;

        onRelease.Invoke();
    }
}
