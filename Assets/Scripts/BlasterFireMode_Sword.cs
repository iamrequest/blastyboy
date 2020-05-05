using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// TODO: Sword collides with grapple point
// TODO: Sword cannot release when grapple is active
public class BlasterFireMode_Sword : BlasterFireMode {
    public GameObject blade;
    private MeshRenderer meshRenderer;

    protected override void Start() {
        base.Start();

        meshRenderer = blade.GetComponent<MeshRenderer>();
        newMainColor = meshRenderer.material.GetColor("MainColor");
    }
    public override void OnFireModeDeselected() {
        blade.SetActive(false);
    }

    public override void OnFireModeSelected() {
    }

    public override void OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        blade.SetActive(true);
        isBlockingGrapple = true;
    }
    public override void OnStateUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        blade.SetActive(false);
        isBlockingGrapple = false;
    }
}
