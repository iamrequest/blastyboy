using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class BlasterFireMode_Sword : BlasterFireMode {
    public GameObject blade;
    private MeshRenderer meshRenderer;

    public float bladeRetractDuration;
    private float lastBladeRetractionTime;

    protected override void Start() {
        base.Start();

        meshRenderer = blade.GetComponent<MeshRenderer>();
        newMainColor = meshRenderer.material.GetColor("MainColor");
    }
    public override void OnFireModeDeselected() {
        // If we've got the blade extended, add a bit of a delay before the next Fire Mode can shoot
        transitionOutDuration = lastBladeRetractionTime - Time.time + bladeRetractDuration;
        if (transitionOutDuration < 0) transitionOutDuration = 0;

        parentBlaster.animator.SetBool("isBladeExtended", false);
    }

    public override void OnFireModeSelected() { 
        lastBladeRetractionTime = Time.time;
    }

    public override void OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        parentBlaster.animator.SetBool("isBladeExtended", true);
        isBlockingGrapple = true;
    }
    public override void OnStateUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        parentBlaster.animator.SetBool("isBladeExtended", false);
        lastBladeRetractionTime = Time.time;
        isBlockingGrapple = false;
    }
}
