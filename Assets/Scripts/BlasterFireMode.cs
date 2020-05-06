using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

// Would be nice: Hook up unity events to all of these methods
// No current use-case, so I'm not adding it right now.
public abstract class BlasterFireMode : MonoBehaviour {
    protected Blaster parentBlaster;
    public bool isBlockingGrapple;
    public float transitionInDuration, transitionOutDuration;

    // The unity thing here is required to set HDR colors
    [ColorUsageAttribute(true,true)]
    public Color newMainColor;

    protected virtual void Start() {
        parentBlaster = GetComponent<Blaster>();
        isBlockingGrapple = false;
    }

    // On Enter / On Exit functions
    public abstract void OnFireModeSelected();
    public abstract void OnFireModeDeselected();

    // SteamVR methods
    public abstract void OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource);
    public abstract void OnStateUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource);
    //public abstract void OnStateUpdate(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState);
}
