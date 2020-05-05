using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class BlasterFireMode_Projectile : BlasterFireMode {
    public GameObject projectilePrefab;
    public float projectileSpeed;
    public AudioClip projectileShotAudio;


    // On Enter / On Exit functions
    public override void OnFireModeSelected() { }
    public override void OnFireModeDeselected() { }

    public override void OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        parentBlaster.audioSource.PlayOneShot(projectileShotAudio);
        parentBlaster.FireProjectile(projectilePrefab, projectileSpeed);
    }

    //public override void OnStateUpdate(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) { }
}
