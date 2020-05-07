using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour {
    const bool DEBUG = true;

    public Transform eyePosition;
    public LayerMask visibleLayers;
    public float visionDistance;
    public float visionRadius;

    // We could have multiple gameobjects that count as the target (child gameobjects count), so we're doing tag comparison rather than gameobject comparison
    public bool IsInSight(Vector3 target, string tag) {
        RaycastHit rayHit;

        if (Physics.Raycast(eyePosition.position, target - eyePosition.position, out rayHit, visionDistance, visibleLayers)) {
            if (!rayHit.collider.CompareTag(tag)) {
                // Something is in the way
                DrawDebugRay(rayHit.point, Color.yellow);
                return false;
            }

            // The target's position is within our vision distance, and it's not occluded by anything
            // Check if it's within our viewing angle
            if (!IsWithingViewingAngle(target)) {
                DrawDebugRay(target, Color.magenta);
                return false;
            }

            // No obstructions, not occluded by vision angle
            DrawDebugRay(target, Color.green);
            return true;
        }

        // Target is too far away, or our layers aren't set up right
        DrawDebugRay(target, Color.red);
        return false;
    }

    private bool IsWithingViewingAngle(Vector3 target) {
        float angleToTarget = Vector3.Angle(target - eyePosition.position, eyePosition.forward);
        return visionRadius > angleToTarget;
    }

    public float DistanceToTarget(Vector3 target) {
        return (target - eyePosition.position).magnitude;
    }

    public void DrawDebugRay(Vector3 target, Color color) {
        if (DEBUG) {
            float drawDistance = Mathf.Min(visionDistance, (target - eyePosition.position).magnitude);
            Debug.DrawRay(eyePosition.position, (target - eyePosition.position).normalized * drawDistance, color);
        }
    }
}
